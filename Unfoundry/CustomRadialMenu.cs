using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unfoundry
{
    public static class CustomRadialMenuManager
    {
        public static bool IsRadialMenuOpen { get; private set; } = false;

        internal static CustomRadialMenu radialMenu = null;

        internal static Sprite spriteCenterBackground = null;
        internal static Material spriteCenterBackgroundMaterial = null;

        internal static Sprite spriteSectorBackground = null;
        internal static Material spriteSectorBackgroundMaterial = null;

        internal static Color highlightedBackgroundColour;
        internal static Color defaultBackgroundColour;

        public static bool CloseMenu(bool runEvents = true)
        {
            if (radialMenu == null) return false;
            if (!IsRadialMenuOpen) return false;

            IsRadialMenuOpen = false;

            var callback = radialMenu.Hide();
            if (runEvents && callback != null) callback();

            return true;
        }

        public static void ShowMenu(CustomRadialMenuOption[] options, Color? highlightedBackgroundColour = null, Color? defaultBackgroundColour = null)
        {
            if (options.Length == 0) return;

            if (spriteCenterBackground == null)
            {
                var transform = ResourceDB.ui_radial_menu.transform.Find("SectorContainer/CenterBG");
                var image = transform.GetComponent<Image>();
                spriteCenterBackground = image.sprite;
                spriteCenterBackgroundMaterial = image.material;
            }

            if (spriteSectorBackground == null)
            {
                var transform = ResourceDB.ui_radial_menu.transform.Find("SectorContainer/Sector/BG");
                var image = transform.GetComponent<Image>();
                spriteSectorBackground = image.sprite;
                spriteSectorBackgroundMaterial = image.material;
            }

            CustomRadialMenuManager.highlightedBackgroundColour = highlightedBackgroundColour ?? new Color(1.0f, 0.5f, 0.0f, 1.0f);
            CustomRadialMenuManager.defaultBackgroundColour = defaultBackgroundColour ?? new Color(1.0f, 1.0f, 1.0f, 1.0f);

            if (radialMenu == null) radialMenu = new CustomRadialMenu(GlobalStateManager.getDefaultUICanvasTransform(true).GetComponent<Canvas>());

            IsRadialMenuOpen = true;

            radialMenu.Show(options);
        }
    }

    public class CustomRadialMenu
    {
        public const int MaxSectors = 16;

        private readonly Canvas canvas;
        private readonly GameObject gameObject;
        private readonly GameObject sectorContainer;
        private readonly TMP_Text centerText;
        private readonly TMP_Text centerSubscriptText;

        private readonly CustomRadialMenuSector[] sectors = new CustomRadialMenuSector[MaxSectors];
        public int VisibleSectorCount { get; private set; } = 0;


        private int highlightedSectorIndex = -1;
        public int HighlightedSectorIndex
        {
            get => highlightedSectorIndex;
            private set
            {
                if (value == highlightedSectorIndex) return;

                if (highlightedSectorIndex >= 0) sectors[highlightedSectorIndex].IsHighlighted = false;

                highlightedSectorIndex = value;

                if (highlightedSectorIndex >= 0)
                {
                    var sector = sectors[highlightedSectorIndex];
                    sector.IsHighlighted = true;
                    centerText.text = sector.Description;
                    centerSubscriptText.text = sector.Subscript;
                }
                else
                {
                    centerText.text = string.Empty;
                    centerSubscriptText.text = string.Empty;
                }
            }
        }

        private bool showing = false;
        private float lastShownTime = float.NaN;
        private bool hiding = false;
        private float lastHideTime = float.NaN;

        private readonly int deadzone;

        public CustomRadialMenu(Canvas canvas)
        {
            this.canvas = canvas;

            deadzone = GlobalStateManager.getUIPreset().radialMenu_deadzonePx;
            gameObject = UIBuilder.BeginWith(canvas.gameObject)
                .Element("Custom Radial Menu")
                    .SetRectTransform(-275, -275, 275, 275, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f)
                    .Element("Sectors")
                        .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                        .Keep(out sectorContainer)
                        .Element("Center Background")
                            .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                            .Component_Image(CustomRadialMenuManager.spriteCenterBackground, CustomRadialMenuManager.spriteCenterBackgroundMaterial, new Color(0.1333f, 0.1333f, 0.1333f, 1.0f))
                        .Done
                    .Done
                    .Element("Center Text")
                        .SetRectTransform(9, 0, -9, 0, 0.5f, 0.5f, 0.25f, 0.25f, 0.75f, 0.75f)
                        .Component_Text("", "OpenSansSemibold SDF", 30, Color.white, TextAlignmentOptions.Center)
                        .Keep(out centerText)
                    .Done
                    .Element("Center Text")
                        .SetRectTransform(9, 0, -9, 0, 0.5f, 0.5f, 0.32f, 0.32f, 0.68f, 0.68f)
                        .Component_Text("", "OpenSansSemibold SDF", 16, Color.white, TextAlignmentOptions.Bottom)
                        .Keep(out centerSubscriptText)
                    .Done
                    .GameObject;

            for (int i = 0; i < MaxSectors; i++)
            {
                var sector = new CustomRadialMenuSector(sectorContainer);
                sectors[i] = sector;
            }

            CommonEvents.OnLateUpdate += Update;
        }

        ~CustomRadialMenu()
        {
            CommonEvents.OnLateUpdate -= Update;
        }

        public void Show(CustomRadialMenuOption[] options)
        {
            if (options.Length == 0) throw new Exception("Empty custom radial menu.");

            var uIPreset = GlobalStateManager.getUIPreset();

            int maxSectors = MaxSectors;
            int optionCount = Mathf.Min(maxSectors, options.Length);

            float width = 1.0f / optionCount;
            for (int index = 0; index < maxSectors; ++index)
            {
                if (index < optionCount)
                {
                    sectors[index].Show(index * width, width, options[index].Icon, options[index].Description, options[index].Subscript, options[index].OnActivated);
                }
                else
                {
                    sectors[index].Hide();
                }
            }

            HighlightedSectorIndex = -1;
            VisibleSectorCount = optionCount;
            centerText.text = "";
            lastShownTime = Time.time;
            showing = true;
            hiding = false;
            gameObject.SetActive(true);
            GlobalStateManager.addCursorRequirementSoft();
            CursorManager.singleton.CenterCursor();
        }

        public CustomRadialMenuOption.ActivatedDelegate Hide(bool isErrorHide = false)
        {
            GlobalStateManager.removeCursorRequirementSoft();

            lastHideTime = Time.time;
            hiding = true;
            showing = false;

            return HighlightedSectorIndex >= 0 ? sectors[HighlightedSectorIndex].OnActivated : null;
        }

        public void Update()
        {
            if (gameObject == null || !gameObject.activeSelf) return;

            if (showing)
            {
                var scaleCurve = ResourceDB.resourceLinker.scaleInCurveUI;
                var animationTime = Time.time - lastShownTime;
                var animationLength = scaleCurve[scaleCurve.length - 1].time;
                if (animationTime >= animationLength)
                {
                    animationTime = animationLength;
                    showing = false;
                }
                var scale = scaleCurve.Evaluate(animationTime);
                gameObject.transform.localScale = new Vector3(scale, scale, 1.0f);
            }
            else if (hiding)
            {
                var scaleCurve = ResourceDB.resourceLinker.scaleOutCurveUI;
                var animationTime = Time.time - lastHideTime;
                var animationLength = scaleCurve[scaleCurve.length - 1].time;
                if (animationTime >= animationLength)
                {
                    animationTime = animationLength;
                    hiding = false;
                    gameObject.SetActive(false);
                }
                var scale = scaleCurve.Evaluate(animationTime);
                gameObject.transform.localScale = new Vector3(scale, scale, 1.0f);

                return;
            }

            if (VisibleSectorCount <= 0) return;

            int width = Screen.width;
            int height = Screen.height;
            var mousePosition = CursorManager.mousePosition;

            int mouseHighlightedSectorIndex = -1;
            var mouseOffset = new Vector2(mousePosition.x - width * 0.5f, mousePosition.y - height * 0.5f) / canvas.scaleFactor;
            if (mouseOffset.sqrMagnitude > deadzone * deadzone)
            {
                var angle = Mathf.Atan2(mouseOffset.x, mouseOffset.y);
                if (angle < 0) angle += 3.141592f * 2;
                mouseHighlightedSectorIndex = Mathf.FloorToInt(angle * VisibleSectorCount / (3.141592f * 2.0f));
                if (mouseHighlightedSectorIndex >= VisibleSectorCount) mouseHighlightedSectorIndex = VisibleSectorCount - 1;
            }

            HighlightedSectorIndex = mouseHighlightedSectorIndex;
        }
    }

    public class CustomRadialMenuSector
    {
        public string Description { get; private set; } = "";
        public string Subscript { get; private set; } = "";

        private readonly GameObject gameObject;
        private readonly RectTransform iconRectTransform;
        private readonly Image backgroundImage;
        private readonly Image iconImage;

        public CustomRadialMenuOption.ActivatedDelegate OnActivated { get; private set; }

        private readonly int iconCenterOffset;

        private bool isHighlighted = false;
        public bool IsHighlighted
        {
            get => isHighlighted;
            internal set
            {
                if (isHighlighted == value) return;
                isHighlighted = value;
                backgroundImage.color = isHighlighted ? CustomRadialMenuManager.highlightedBackgroundColour : CustomRadialMenuManager.defaultBackgroundColour;
            }
        }

        public CustomRadialMenuSector(GameObject container)
        {
            iconCenterOffset = GlobalStateManager.getUIPreset().radialMenu_iconCenterOffsetPx;

            gameObject = UIBuilder.BeginWith(container)
                .Element("Sector")
                    .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                    .Element("Background")
                        .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                        .Component_ImageFilled(CustomRadialMenuManager.spriteSectorBackground, CustomRadialMenuManager.spriteSectorBackgroundMaterial, Image.FillMethod.Radial360, true, true, 2, 1.0f, CustomRadialMenuManager.defaultBackgroundColour)
                        .Keep(out backgroundImage)
                    .Done
                    .Element("Icon")
                        .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f)
                        .Component_Image((Sprite)null)
                        .Keep(out iconImage)
                        .Keep(out iconRectTransform)
                    .Done
                    .GameObject;
        }

        public void Show(float start, float width, Sprite icon, string description, string subscript, CustomRadialMenuOption.ActivatedDelegate onActivated)
        {
            Description = description;
            Subscript = subscript;
            OnActivated = onActivated;

            backgroundImage.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -start * 360.0f);
            backgroundImage.fillAmount = width;
            backgroundImage.color = CustomRadialMenuManager.defaultBackgroundColour;

            var iconAngle = (start + width * 0.5f) * (3.141592f * 2.0f);
            iconImage.sprite = icon;
            var iconCenter = new Vector2(Mathf.Sin(iconAngle), Mathf.Cos(iconAngle)) * iconCenterOffset;
            iconRectTransform.offsetMin = iconCenter - new Vector2(42.5f, 42.5f);
            iconRectTransform.offsetMax = iconCenter + new Vector2(42.5f, 42.5f);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    public class CustomRadialMenuOption
    {
        public delegate void ActivatedDelegate();
        public delegate bool IsEnabledDelegate();

        public string Description { get; private set; }
        public Sprite Icon { get; private set; }
        public string Subscript { get; private set; }
        public ActivatedDelegate OnActivated { get; private set; }
        public IsEnabledDelegate IsEnabledHandler { get; private set; }

        public bool IsEnabled => IsEnabledHandler == null || IsEnabledHandler.Invoke();

        public CustomRadialMenuOption(string description, Sprite icon, ActivatedDelegate onActivated, IsEnabledDelegate isEnabledHandler = null)
            : this(description, icon, "", onActivated, isEnabledHandler)
        {
        }

        public CustomRadialMenuOption(string description, Sprite icon, string subscript, ActivatedDelegate onActivated, IsEnabledDelegate isEnabledHandler = null)
        {
            Description = description;
            Icon = icon;
            Subscript = subscript;
            OnActivated = onActivated;
            IsEnabledHandler = isEnabledHandler;
        }
    }
}
