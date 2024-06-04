using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unfoundry
{
    public class UIBuilder
    {
        protected UIBuilder(GameObject gameObject, UIBuilder parent)
        {
            GameObject = gameObject;
            Parent = parent;
        }

        protected UIBuilder() : this(null, null)
        {
        }

        public static UIBuilder BeginWith(GameObject gameObject)
        {
            UIBuilder builder = new UIBuilder
            {
                GameObject = gameObject,
                Parent = null
            };

            return builder;
        }

        public delegate void OnClickDelegate();
        public delegate void OnValueChangedDelegate(float value);
        public delegate void GenericUpdateDelegate();
        public delegate void ElementUpdateDelegate(GameObject gameObject);
        public delegate bool VisibilityUpdateDelegate();
        public delegate void DoDelegate(UIBuilder builder);

        public GameObject GameObject { get; private set; }
        public UIBuilder Parent { get; private set; }
        public UIBuilder Done => Parent;
        public void End(bool validate = true)
        {
            if (validate && Parent != null && Parent.GameObject != null) throw new Exception(string.Format("Invalid UI Builder End: {0}", Parent.GameObject.name));
        }

        public UIBuilder Keep(out GameObject gameObject)
        {
            gameObject = GameObject;
            return this;
        }

        public UIBuilder With(Action<GameObject> action)
        {
            action(GameObject);
            return this;
        }

        public UIBuilder WithComponent<T>(Action<T> action)
        {
            action(GameObject.GetComponent<T>());
            return this;
        }

        public UIBuilder Keep<C>(out C component) where C : Component
        {
            component = GameObject.GetComponent<C>();
            return this;
        }

        public UIBuilder SetOffsets(float offsetMinX, float offsetMinY, float offsetMaxX, float offsetMaxY)
        {
            var transform = (RectTransform)GameObject.transform;
            transform.offsetMin = new Vector2(offsetMinX, offsetMinY);
            transform.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
            return this;
        }

        public UIBuilder SetPivot(float pivotX, float pivotY)
        {
            var transform = (RectTransform)GameObject.transform;
            transform.pivot = new Vector2(pivotX, pivotY);
            return this;
        }

        public UIBuilder SetAnchor(float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            var transform = (RectTransform)GameObject.transform;
            transform.anchorMin = new Vector2(anchorMinX, anchorMinY);
            transform.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
            return this;
        }

        public UIBuilder SetRectTransform(float offsetMinX, float offsetMinY, float offsetMaxX, float offsetMaxY, float pivotX, float pivotY, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
        {
            SetPivot(pivotX, pivotY);
            SetAnchor(anchorMinX, anchorMinY, anchorMaxX, anchorMaxY);
            SetOffsets(offsetMinX, offsetMinY, offsetMaxX, offsetMaxY);
            return this;
        }

        public UIBuilder SetRotation(float degrees)
        {
            var transform = (RectTransform)GameObject.transform;
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, degrees);
            return this;
        }

        public UIBuilder SetHorizontalLayout(RectOffset padding, float spacing, TextAnchor childAlignment, bool reverseArrangement, bool childControlWidth, bool childControlHeight, bool childForceExpandWidth, bool childForceExpandHeight, bool childScaleWidth, bool childScaleHeight)
        {
            var component = GameObject.AddComponent<HorizontalLayoutGroup>();
            component.padding = padding;
            component.spacing = spacing;
            component.childAlignment = childAlignment;
            component.reverseArrangement = reverseArrangement;
            component.childControlWidth = childControlWidth;
            component.childControlHeight = childControlHeight;
            component.childForceExpandWidth = childForceExpandWidth;
            component.childForceExpandHeight = childForceExpandHeight;
            component.childScaleWidth = childScaleWidth;
            component.childScaleHeight = childScaleHeight;

            return this;
        }

        public UIBuilder SetVerticalLayout(RectOffset padding, float spacing, TextAnchor childAlignment, bool reverseArrangement, bool childControlWidth, bool childControlHeight, bool childForceExpandWidth, bool childForceExpandHeight, bool childScaleWidth, bool childScaleHeight)
        {
            var component = GameObject.AddComponent<VerticalLayoutGroup>();
            component.padding = padding;
            component.spacing = spacing;
            component.childAlignment = childAlignment;
            component.reverseArrangement = reverseArrangement;
            component.childControlWidth = childControlWidth;
            component.childControlHeight = childControlHeight;
            component.childForceExpandWidth = childForceExpandWidth;
            component.childForceExpandHeight = childForceExpandHeight;
            component.childScaleWidth = childScaleWidth;
            component.childScaleHeight = childScaleHeight;

            return this;
        }

        public UIBuilder SetGridLayout(RectOffset padding, Vector2 cellSize, Vector2 spacing, GridLayoutGroup.Corner startCorner = GridLayoutGroup.Corner.UpperLeft, GridLayoutGroup.Axis startAxis = GridLayoutGroup.Axis.Horizontal, TextAnchor childAlignment = TextAnchor.UpperLeft, GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.Flexible)
        {
            var component = GameObject.AddComponent<GridLayoutGroup>();
            component.padding = padding;
            component.cellSize = cellSize;
            component.spacing = spacing;
            component.startCorner = startCorner;
            component.startAxis = startAxis;
            component.childAlignment = childAlignment;
            component.constraint = constraint;

            return this;
        }

        public UIBuilder AutoSize(ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.PreferredSize)
        {
            var component = GameObject.AddComponent<ContentSizeFitter>();
            component.horizontalFit = horizontalFit;
            component.verticalFit = verticalFit;

            return this;
        }

        public UIBuilder Element(string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            if (GameObject != null) gameObject.transform.SetParent(GameObject.transform, false);

            UIBuilder elementBuilder = new UIBuilder
            {
                GameObject = gameObject,
                Parent = this
            };

            return elementBuilder;
        }

        public UIBuilder Element_Panel(string name, string textureName, Color color, Vector4 border, Image.Type imageType = Image.Type.Sliced)
        {
            return Element(name)
                .Component_Image(textureName, color, imageType, border)
                .Component<Outline>();
        }

        public UIBuilder Element_PanelAutoSize(string name, string textureName, Color color, Vector4 border, Image.Type imageType = Image.Type.Sliced, ContentSizeFitter.FitMode horizontalFit = ContentSizeFitter.FitMode.MinSize, ContentSizeFitter.FitMode verticalFit = ContentSizeFitter.FitMode.PreferredSize)
        {
            return Element(name)
                .Component_Image(textureName, color, imageType, border)
                .Component<Outline>()
                .Component_ContentSizeFitter(horizontalFit, verticalFit);
        }

        public UIBuilder Element_Header(string name, string textureName, Color color, Vector4 border, Image.Type imageType = Image.Type.Sliced)
        {
            return Element(name)
                .Component_Image(textureName, color, imageType, border);
        }

        public UIBuilder Element_Button(string name, Sprite sprite, Color color, Vector4 border, Image.Type imageType = Image.Type.Sliced)
        {
            return Element(name)
                .Component_Image(sprite, color, imageType, border)
                .Component<Button>();
        }

        public UIBuilder Element_Button(string name, string textureName, Color color, Vector4 border, Image.Type imageType = Image.Type.Sliced)
        {
            return Element(name)
                .Component_Image(textureName, color, imageType, border)
                .Component<Button>();
        }

        public UIBuilder Element_Label(string name, string text, float minWidth = 100.0f, float flexibleWidth = 0.0f, Color? color = null)
        {
            return Element(name)
                .Layout()
                    .MinWidth(minWidth)
                    .PreferredWidth(minWidth)
                    .FlexibleWidth(flexibleWidth)
                .Done
                .Component_Text(text, "OpenSansSemibold SDF", 18.0f, color ?? Color.white, TextAlignmentOptions.MidlineLeft);
        }

        public UIBuilder Element_TextButton(string name, string text, Color? color = null)
        {
            return Element_Button(name, "corner_cut", Color.white, new Vector4(10.0f, 1.0f, 2.0f, 10.0f))
                .SetTransitionColors(new Color(0.2f, 0.2f, 0.2f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.222f, 0.667f, 1.0f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                .Layout()
                    .MinWidth(40)
                    .MinHeight(40)
                    .FlexibleWidth(1.0f)
                .Done
                .Element("Text")
                    .SetRectTransform(0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .Component_Text(text, "OpenSansSemibold SDF", 18.0f, color ?? Color.white, TextAlignmentOptions.Center)
                .Done;
        }

        public UIBuilder Element_ImageButton(string name, string textureName, int imageWidth = 36, int imageHeight = 36, float rotation = 0.0f)
        {
            return Element_Button(name, "corner_cut", Color.white, new Vector4(10.0f, 1.0f, 2.0f, 10.0f))
                .SetTransitionColors(new Color(0.2f, 0.2f, 0.2f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.222f, 0.667f, 1.0f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                .Layout()
                    .MinWidth(imageWidth + 10)
                    .MinHeight(imageHeight + 10)
                    .PreferredWidth(imageWidth + 10)
                    .PreferredHeight(imageHeight + 10)
                .Done
                .Element("Image")
                    .SetRectTransform(5.0f, 5.0f, -5.0f, -5.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .SetRotation(rotation)
                    .Component_Image(textureName, Color.white, Image.Type.Sliced, Vector4.zero)
                .Done;
        }

        public UIBuilder Element_ImageTextButton(string name, string text, string textureName, Color? color = null, int imageWidth = 36, int imageHeight = 36, float rotation = 0.0f)
        {
            return Element_Button(name, "corner_cut", Color.white, new Vector4(10.0f, 1.0f, 2.0f, 10.0f))
                .SetTransitionColors(new Color(0.2f, 0.2f, 0.2f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.222f, 0.667f, 1.0f, 1.0f), new Color(0.0f, 0.6f, 1.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                .Layout()
                    .MinWidth(imageWidth + 10)
                    .MinHeight(imageHeight + 10)
                    .FlexibleWidth(1.0f)
                .Done
                .Element("Text")
                    .SetRectTransform(0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .Component_Text(text, "OpenSansSemibold SDF", 18.0f, color ?? Color.white, TextAlignmentOptions.Center)
                .Done
                .Element("Image")
                    .SetRectTransform(5.0f, -imageHeight * 0.5f, 5.0f + imageWidth, imageHeight * 0.5f, 0.0f, 0.5f, 0.0f, 0.5f, 0.0f, 0.5f)
                    .SetRotation(rotation)
                    .Component_Image(textureName)
                .Done;
        }

        public UIBuilder Element_Slider(string name, float value, float rangeFrom, float rangeTo, OnValueChangedDelegate onValueChanged = null)
        {
            return Element(name)
                .Element("Background")
                    .SetRectTransform(10.0f, -10.0f, -10.0f, 10.0f, 0.5f, 0.5f, 0.0f, 0.5f, 1.0f, 0.5f)
                    .Component_Image("solid_square_white", Color.white)
                    .Component<Outline>()
                .Done
                .Element("Fill Area")
                    .SetRectTransform(10.0f, 0.0f, -10.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.25f, 1.0f, 0.75f)
                    .Element("Fill")
                        .Keep(out RectTransform fillRect)
                        .SetRectTransform(-5.0f, 0.0f, 5.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 0.1919192f, 1.0f)
                        .Component_Image("solid_square_white", new Color(0.0f, 0.6f, 1.0f))
                    .Done
                .Done
                .Element("Handle Slide Area")
                    .SetRectTransform(10.0f, 0.0f, -10.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .Element("Handle")
                        .Keep(out RectTransform handleRect)
                        .SetRectTransform(-12.5f, 0.0f, 12.5f, 0.0f, 0.5f, 0.5f, 0.1919192f, 0.0f, 0.1919192f, 1.0f)
                        .Component_Image("solid_square_white", new Color(0.0f, 0.6f, 1.0f))
                        .Component<Outline>()
                    .Done
                .Done
                .Component_Slider(value, rangeFrom, rangeTo, fillRect, handleRect, onValueChanged);
        }

        public UIBuilder Element_InputField(string name, string text, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard)
        {
            return Element(name)
                .Component_Image("corner_cut", Color.white, Image.Type.Sliced, new Vector4(10.0f, 1.0f, 2.0f, 10.0f))
                .Component<TMP_InputField>()
                .Layout()
                    .MinWidth(40)
                    .MinHeight(40)
                    .FlexibleWidth(1.0f)
                .Done
                .Element("TextArea")
                    .SetRectTransform(10, 6, -10, -6, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .Component<RectMask2D>()
                    .Element("Text")
                        .SetRectTransform(0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                        .Component_Text(text, "OpenSansSemibold SDF", 18.0f, Color.black, TextAlignmentOptions.MidlineLeft)
                    .Done
                .Done
                .WithComponent((TMP_InputField inputField) =>
                {
                    inputField.textComponent = inputField.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                    inputField.text = text;
                    inputField.selectionColor = new Color(0.6f, 0.6f, 1.0f, 1.0f);
                    inputField.enabled = false; // Hack to make caret appear
                    inputField.enabled = true;
                    inputField.contentType = contentType;
                });
        }

        private static GameObject _scrollBoxPrefab = null;
        public UIBuilder Element_ScrollBox(string name, Action<UIBuilder> contentBuilder)
        {
            if (_scrollBoxPrefab == null)
            {
                _scrollBoxPrefab = Plugin.GetAsset<GameObject>("ScrollBox");
                if (_scrollBoxPrefab == null) throw new Exception("Failed to load ScrollBox prefab");
            }

            var gameObject = UnityEngine.Object.Instantiate(_scrollBoxPrefab);
            if (GameObject != null) gameObject.transform.SetParent(GameObject.transform, false);
            var content = gameObject.transform.Find("Viewport/Content");

            contentBuilder(new UIBuilder
            {
                GameObject = content.gameObject,
                Parent = this
            });

            return new UIBuilder
            {
                GameObject = gameObject,
                Parent = this
            };
        }

        public UIBuilder SetTransitionColors(Color normalColor, Color highlightedColor, Color pressedColor, Color selectedColor, Color disabledColor, float colorMultiplier, float fadeDuration)
        {
            var selectable = GameObject.GetComponent<Selectable>();
            if (selectable != null)
            {
                var colors = new ColorBlock
                {
                    normalColor = normalColor,
                    highlightedColor = highlightedColor,
                    pressedColor = pressedColor,
                    selectedColor = selectedColor,
                    disabledColor = disabledColor,
                    colorMultiplier = colorMultiplier,
                    fadeDuration = fadeDuration
                };

                selectable.transition = Selectable.Transition.ColorTint;
                selectable.colors = colors;
            }

            return this;
        }

        public UIBuilder SetOnClick(OnClickDelegate action)
        {
            var button = GameObject.GetComponent<Button>();
            button?.onClick.AddListener(new UnityAction(action));

            return this;
        }

        public UIBuilder Component<C>() where C : Component
        {
            GameObject.AddComponent<C>();
            return this;
        }

        public UIBuilder Component_Text(string text, string fontName, float fontSize, Color fontColor, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var component = GameObject.AddComponent<TextMeshProUGUI>();
            component.text = text;
            component.font = ResourceExt.FindFont(fontName);
            component.fontSize = fontSize;
            component.color = fontColor;
            component.alignment = alignment;
            return this;
        }

        public UIBuilder Component_Image(string textureName, Color? color = null, Image.Type imageType = Image.Type.Simple, Vector4? border = null)
        {
            return Component_Image(GetSprite(textureName, border ?? Vector4.zero), color, imageType, border);
        }

        public UIBuilder Component_Image(Sprite sprite, Color? color = null, Image.Type imageType = Image.Type.Simple, Vector4? border = null)
        {
            var component = GameObject.AddComponent<Image>();
            component.type = imageType;
            component.sprite = sprite;
            component.color = color ?? Color.white;
            return this;
        }

        public UIBuilder Component_Image(Sprite sprite, Material material, Color? color = null, Image.Type imageType = Image.Type.Simple, Vector4? border = null)
        {
            var component = GameObject.AddComponent<Image>();
            component.type = imageType;
            component.sprite = sprite;
            component.material = material;
            component.color = color ?? Color.white;
            return this;
        }

        public UIBuilder Component_ImageFilled(string textureName, Image.FillMethod fillMethod, bool fillCenter, bool fillClockwise, int fillOrigin, float fillAmount, Color? color = null)
        {
            return Component_ImageFilled(GetSprite(textureName, Vector4.zero), fillMethod, fillCenter, fillClockwise, fillOrigin, fillAmount, color);
        }

        public UIBuilder Component_ImageFilled(Sprite sprite, Image.FillMethod fillMethod, bool fillCenter, bool fillClockwise, int fillOrigin, float fillAmount, Color? color = null)
        {
            var component = GameObject.AddComponent<Image>();
            component.type = Image.Type.Filled;
            component.sprite = sprite;
            component.fillMethod = fillMethod;
            component.fillCenter = fillCenter;
            component.fillClockwise = fillClockwise;
            component.fillOrigin = fillOrigin;
            component.fillAmount = fillAmount;
            component.color = color ?? Color.white;
            return this;
        }

        public UIBuilder Component_ImageFilled(Sprite sprite, Material material, Image.FillMethod fillMethod, bool fillCenter, bool fillClockwise, int fillOrigin, float fillAmount, Color? color = null)
        {
            var component = GameObject.AddComponent<Image>();
            component.type = Image.Type.Filled;
            component.sprite = sprite;
            component.material = material;
            component.fillMethod = fillMethod;
            component.fillCenter = fillCenter;
            component.fillClockwise = fillClockwise;
            component.fillOrigin = fillOrigin;
            component.fillAmount = fillAmount;
            component.color = color ?? Color.white;
            return this;
        }

        public UIBuilder Component_Slider(float value, float rangeFrom, float rangeTo, RectTransform fillRect, RectTransform handleRect, OnValueChangedDelegate onValueChanged = null)
        {
            var component = GameObject.AddComponent<Slider>();
            component.minValue = rangeFrom;
            component.maxValue = rangeTo;
            component.value = value;
            component.fillRect = fillRect;
            component.handleRect = handleRect;
            if (onValueChanged != null) component.onValueChanged.AddListener(new UnityAction<float>(onValueChanged));
            return this;
        }

        public UIBuilder Component_Tooltip(string text)
        {
            var component = GameObject.AddComponent<TooltipTrigger>();
            component.tooltipText = text;
            return this;
        }

        public UIBuilder Component_ContentSizeFitter(ContentSizeFitter.FitMode horizontalFit, ContentSizeFitter.FitMode verticalFit)
        {
            var component = GameObject.AddComponent<ContentSizeFitter>();
            component.horizontalFit = horizontalFit;
            component.verticalFit = verticalFit;
            return this;
        }

        public UIBuilder Do(DoDelegate action)
        {
            action(this);
            return this;
        }

        public LayoutElementBuilder Layout()
        {
            return new LayoutElementBuilder(this);
        }

        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        private static Sprite GetSprite(string textureName, Vector4 border)
        {
            if (sprites.TryGetValue(textureName, out Sprite sprite)) return sprite;

            var texture = ResourceExt.FindTexture(textureName);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, border);
        }

        public class LayoutElementBuilder
        {
            private readonly LayoutElement component;

            internal LayoutElementBuilder(UIBuilder parent)
            {
                Parent = parent;
                if (!Parent.GameObject.TryGetComponent(out component))
                {
                    component = Parent.GameObject.AddComponent<LayoutElement>();
                }
            }

            public LayoutElementBuilder MinWidth(float value) { component.minWidth = value; return this; }
            public LayoutElementBuilder MinHeight(float value) { component.minHeight = value; return this; }
            public LayoutElementBuilder PreferredWidth(float value) { component.preferredWidth = value; return this; }
            public LayoutElementBuilder PreferredHeight(float value) { component.preferredHeight = value; return this; }
            public LayoutElementBuilder FlexibleWidth(float value) { component.flexibleWidth = value; return this; }
            public LayoutElementBuilder FlexibleHeight(float value) { component.flexibleHeight = value; return this; }

            public UIBuilder Parent { get; private set; }
            public UIBuilder Done => Parent;
        }

        public UIBuilder Updater(List<GenericUpdateDelegate> list, ElementUpdateDelegate updater)
        {
            var go = GameObject;
            list.Add(() => updater(go));
            return this;
        }

        public UIBuilder Updater(List<GenericUpdateDelegate> list, VisibilityUpdateDelegate updater)
        {
            var go = GameObject;
            list.Add(() => go.SetActive(updater()));
            return this;
        }

        public UIBuilder Updater<C>(List<GenericUpdateDelegate> list, VisibilityUpdateDelegate updater) where C : Button
        {
            var component = GameObject.GetComponent<C>();
            list.Add(() => component.interactable = updater());
            return this;
        }
    }
}
