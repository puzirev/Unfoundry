using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Unfoundry.Plugin;

namespace Unfoundry
{
    public static class ActionManager
    {
        public delegate void BuildEventDelegate(ulong entityId);
        private static Dictionary<Vector3Int, BuildEventDelegate> buildEvents = new Dictionary<Vector3Int, BuildEventDelegate>();

        public static int MaxQueuedEventsPerFrame = 20;
        public delegate void QueuedEventDelegate();
        private static Queue<QueuedEventDelegate> queuedEvents = new Queue<QueuedEventDelegate>();

        private static TimedAction timedActions = null;
        private static TimedAction timedActionFreeList = null;

        public static string StatusText { get; private set; } = "";

        public static void AddBuildEvent(BuildEntityEvent target, BuildEventDelegate handler)
        {
            Vector3Int worldPos = new Vector3Int(target.worldBuildPos[0], target.worldBuildPos[1], target.worldBuildPos[2]);
            if (buildEvents.ContainsKey(worldPos))
            {
                Debug.LogWarning((string)$"Build event already exists at {worldPos}");
                return;
            }

            buildEvents[worldPos] = handler;
        }

        public static void RemoveBuildEvent(BuildEntityEvent target)
        {
            Vector3Int worldPos = new Vector3Int(target.worldBuildPos[0], target.worldBuildPos[1], target.worldBuildPos[2]);
            if (!buildEvents.ContainsKey(worldPos))
            {
                return;
            }

            buildEvents.Remove(worldPos);
        }

        public static void InvokeBuildEvent(BuildEntityEvent target, ulong entityId)
        {
            Vector3Int worldPos = new Vector3Int(target.worldBuildPos[0], target.worldBuildPos[1], target.worldBuildPos[2]);
            BuildEventDelegate handler;
            if (buildEvents.TryGetValue(worldPos, out handler))
            {
                handler(entityId);
            }
        }

        public static void InvokeAndRemoveBuildEvent(BuildEntityEvent target, ulong entityId)
        {
            Vector3Int worldPos = new Vector3Int(target.worldBuildPos[0], target.worldBuildPos[1], target.worldBuildPos[2]);
            BuildEventDelegate handler;
            if (buildEvents.TryGetValue(worldPos, out handler))
            {
                handler(entityId);
                buildEvents.Remove(worldPos);
            }
        }

        public static void AddQueuedEvent(QueuedEventDelegate queuedEvent)
        {
            queuedEvents.Enqueue(queuedEvent);
        }

        public static void AddTimedAction(float time, float randomAdd, Action action)
        {
            AddTimedAction(time + UnityEngine.Random.value * randomAdd, action);
        }

        public static void AddTimedAction(float time, Action action)
        {
            if (timedActionFreeList == null)
            {
                BubbleTimedAction(new TimedAction(time, action, null));
            }
            else
            {
                var timedAction = timedActionFreeList;
                timedActionFreeList = timedActionFreeList.Next;

                timedAction.Reinitialize(time, action);
                BubbleTimedAction(timedAction);
            }
        }

        private static void BubbleTimedAction(TimedAction node)
        {
            if (timedActions == null)
            {
                timedActions = node;
                return;
            }

            if (timedActions.Time >= node.Time)
            {
                node.Next = timedActions;
                timedActions = node;
                return;
            }

            var prev = timedActions;
            while (prev.Next != null && prev.Next.Time < node.Time) prev = prev.Next;
            node.Next = prev.Next;
            prev.Next = node;
        }

        public static void Update()
        {
            int toProcess = queuedEvents.Count;
            if (toProcess > MaxQueuedEventsPerFrame) toProcess = MaxQueuedEventsPerFrame;
            while (toProcess-- > 0)
            {
                queuedEvents.Dequeue().Invoke();
            }

            var time = Time.time;
            while (timedActions != null && timedActions.Time <= time)
            {
                var timedAction = timedActions;
                timedActions = timedActions.Next;

                timedAction.Action.Invoke();

                timedAction.Next = timedActionFreeList;
                timedActionFreeList = timedAction;
                timedAction.Action = null;
            }

            StatusText = (queuedEvents.Count > 0) ? $"Tasks: {queuedEvents.Count}" : "";
        }

        public static void OnGameInitializationDone()
        {
            buildEvents.Clear();

            queuedEvents.Clear();

            timedActions = null;
            timedActionFreeList = null;

            StatusText = "";
        }

        private class TimedAction
        {
            public float Time { get; private set; }
            public Action Action { get; internal set; }
            public TimedAction Next { get; internal set; }

            public TimedAction(float time, Action action, TimedAction next)
            {
                Time = time;
                Action = action;
                Next = next;
            }

            public void Reinitialize(float time, Action action, TimedAction next = null)
            {
                Time = time;
                Action = action;
                Next = next;
            }
        }
    }
}