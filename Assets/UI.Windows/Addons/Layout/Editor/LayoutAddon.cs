﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI.Windows.Plugins.Flow;
using UnityEditor.UI.Windows.Plugins.Flow;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using ME;
using UnityEditorInternal;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEditor.UI.Windows.Plugins.Flow.Layout;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows;
using UnityEngine.UI.Windows.Components;

namespace UnityEditor.UI.Windows.Plugins.Layout {

	public class Layout : FlowAddon {

		public class Styles {

			public GUISkin skin;
			public GUIStyle backLock;
			public GUIStyle content;
			public GUIStyle contentScreen;
			public GUIStyle closeButton;
			public GUIStyle listButton;
			public GUIStyle listButtonSelected;
			public GUIStyle listTag;
			public GUIStyle objectField;

			public Styles() {

				this.skin = Resources.Load("UI.Windows/Flow/Styles/Skin" + (EditorGUIUtility.isProSkin == true ? "Dark" : "Light")) as GUISkin;
				if (this.skin != null) {

					this.backLock = this.skin.FindStyle("LayoutBackLock");
					this.content = this.skin.FindStyle("LayoutContent");
					this.contentScreen = this.skin.FindStyle("LayoutContentScreen");
					this.closeButton = this.skin.FindStyle("CloseButton");
					this.listButton = this.skin.FindStyle("ListButton");
					this.listButtonSelected = this.skin.FindStyle("ListButtonSelected");

					this.listTag = new GUIStyle(this.skin.FindStyle("ListButton"));
					this.listTag.alignment = TextAnchor.MiddleRight;
					this.objectField = this.skin.FindStyle("ObjectField");

				}

			}

		}

		public static Styles styles = new Styles();
		public const float MARGIN = 40f;
		public const float OFFSET = 4f;

		//private FD.FlowWindow window;
		private LayoutWindowType screen;
		private bool opened = false;
		private IPreviewEditor editor;
		private WindowLayoutElement element;
		private Vector2 listScrollPosition;
		private Vector2 settingsScrollPosition;

		public override void OnFlowSettingsGUI() {
			
			GUILayout.Label(FlowAddon.MODULE_INSTALLED, EditorStyles.centeredGreyMiniLabel);

		}
		
		private UnityEngine.UI.Windows.Types.Layout.Component component;
		private UnityEngine.UI.Windows.Types.Layout.Component hovered;
		private float allListHeight;
		private List<SerializedProperty> props = new List<SerializedProperty>();
		private List<WindowLayoutElement> highlighted = new List<WindowLayoutElement>();
		public override void OnGUI() {

			if (this.opened == true) {

				const float settingsWidth = 250f;
				const float listHeight = 200f;
				const float padding = 5f;
				const float closeSize = 30f;
				const float scrollWidth = 16f;

				var rect = new Rect(0f, -OFFSET, Screen.width, Screen.height - OFFSET);
				var rectContent = new Rect(rect.x + MARGIN + settingsWidth + padding, rect.y + MARGIN, rect.width - MARGIN * 2f - padding - settingsWidth, rect.height - MARGIN * 2f - FlowSystemEditorWindow.TOOLBAR_HEIGHT);
				var rectList = new Rect(MARGIN, rect.y + MARGIN, settingsWidth, listHeight - padding);
				var rectSettings = new Rect(MARGIN, rect.y + MARGIN + listHeight, settingsWidth, rect.height - MARGIN * 2f - FlowSystemEditorWindow.TOOLBAR_HEIGHT - listHeight);
				var rectCloseButton = new Rect(rect.x + rect.width - closeSize, rect.y, closeSize, closeSize);

				GUI.Box(rect, string.Empty, Layout.styles.backLock);
				GUI.Box(rectList, string.Empty, Layout.styles.content);
				GUI.Box(rectSettings, string.Empty, Layout.styles.content);
				GUI.Box(rectContent, string.Empty, Layout.styles.contentScreen);
				
				GUI.BeginGroup(rectSettings);
				{
					if (this.component != null) {
						
						const float offsetTop = 50f;
						
						var viewRect = new Rect(0f, 0f, rectSettings.width, 0f);
						var scrollView = new Rect(0f, 0f + offsetTop, rectSettings.width, rectSettings.height - offsetTop);
						
						System.Action<WindowComponent> onChange = (WindowComponent component) => {

							//Debug.Log(component + "!=" + this.component.component);
							if (component != this.component.component) {
								
								this.component.component = component;
								this.component.componentParametersEditor = null;
								this.component.componentParameters = this.component.OnComponentChanged(this.screen, component);
								
							}

						};

						var c = EditorGUI.ObjectField(new Rect(5f, 5f, viewRect.width - 40f - 5f * 2f, 16f), this.component.component, typeof(WindowComponent), allowSceneObjects: false) as WindowComponent;
						if (c != this.component.component) {

							onChange(c);

						}

						var nRect = new Rect(viewRect.width - 40f, 5f, 40f - 5f, 16f);
						GUILayoutExt.DrawComponentChooser(nRect, this.screen.gameObject, this.component.component, (component) => {

							onChange(component);

						});
						
						if (this.component.component != null) {

							nRect.x = 5f;
							nRect.width = viewRect.width - 5f * 2f;
							nRect.y += nRect.height + 5f;
							this.component.sortingOrder = EditorGUI.IntField(nRect, new GUIContent("Sorting Order"), this.component.sortingOrder);

							var editor = this.component.componentParametersEditor;
							if (editor == null && this.component.componentParameters != null) {
								
								var e = Editor.CreateEditor(this.component.componentParameters) as IParametersEditor;
								this.component.componentParametersEditor = e;
								
							}

							if (editor != null) {

								var h = Mathf.Max(scrollView.height, (editor == null) ? 0f : editor.GetHeight());
								viewRect = new Rect(scrollView.x, scrollView.y, viewRect.width - scrollWidth, h);

								var oldSkin = GUI.skin;
								GUI.skin = FlowSystemEditorWindow.defaultSkin;
								this.settingsScrollPosition = GUI.BeginScrollView(scrollView, this.settingsScrollPosition, viewRect, false, true);
								GUI.skin = oldSkin;
								{
									if (editor != null) {
										
										EditorGUIUtility.labelWidth = 100f;
										++EditorGUI.indentLevel;
										editor.OnParametersGUI(viewRect);
										--EditorGUI.indentLevel;
										EditorGUIUtility.LookLikeControls();
										
									}
								}
								GUI.EndScrollView();

							} else {

								GUI.Label(new Rect(0f, 0f, rectSettings.width - scrollWidth, rectSettings.height), "Selected component have no parameters", EditorStyles.centeredGreyMiniLabel);

							}

						}

					} else {
						
						GUI.Label(new Rect(0f, 0f, rectSettings.width - scrollWidth, rectSettings.height), "Select an Element", EditorStyles.centeredGreyMiniLabel);
						
					}
				}
				GUI.EndGroup();

				GUI.BeginGroup(rectList);
				{

					const float itemHeight = 30f;

					this.highlighted.Clear();

					var viewRect = new Rect(0f, 0f, rectList.width - scrollWidth, 0f);
					this.allListHeight = 0f;
					for (int i = 0; i < this.props.Count; ++i) {
						
						var root = this.screen.layout.layout.GetRootByTag(this.screen.layout.components[i].tag);
						if (root.showInComponentsList == false) continue;

						if (this.screen.layout.components[i].component == null) {

							this.highlighted.Add(root);

						}

						this.allListHeight += itemHeight;
						
					}

					viewRect.height = Mathf.Max(rectList.height, this.allListHeight);

					var oldSkin = GUI.skin;
					GUI.skin = FlowSystemEditorWindow.defaultSkin;
					this.listScrollPosition = GUI.BeginScrollView(new Rect(0f, 0f, rectList.width, rectList.height), this.listScrollPosition, viewRect, false, true);
					GUI.skin = oldSkin;
					{
						GUI.BeginGroup(viewRect);
						{
							var h = 0f;
							this.hovered = null;
							for (int i = 0; i < this.props.Count; ++i) {

								var root = this.screen.layout.layout.GetRootByTag(this.screen.layout.components[i].tag);
								if (root.showInComponentsList == false) continue;

								var r = new Rect(0f, h, viewRect.width, itemHeight);
								h += r.height;

								var isSelected = (this.element == root);
								if (isSelected == true) {

									GUI.Label(r, this.screen.layout.components[i].description, Layout.styles.listButtonSelected);

								} else {
									
									//r.width -= scrollWidth;
									if (GUI.Button(r, this.screen.layout.components[i].description, Layout.styles.listButton) == true) {

										this.component = this.screen.layout.components.FirstOrDefault(c => c.tag == root.tag);
										this.element = root;

									}

									var inRect = rectList.Contains(Event.current.mousePosition - this.listScrollPosition + Vector2.up * 40f);
									if (GUI.enabled == true) EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
									if (r.Contains(Event.current.mousePosition) == true && inRect == true) {
										
										this.hovered = this.screen.layout.components[i];
										
									}
									//r.width += scrollWidth;

								}

								//r.width -= scrollWidth;
								GUI.Label(r, this.screen.layout.components[i].tag.ToString(), Layout.styles.listTag);

							}
						}
						GUI.EndGroup();
					}
					GUI.EndScrollView();

				}
				GUI.EndGroup();

				var selected = (this.hovered != null) ? this.screen.layout.layout.GetRootByTag(this.hovered.tag) : this.element;
				this.editor.OnPreviewGUI(Color.white, rectContent, Layout.styles.content, selected: selected, onSelection: (element) => {

					this.component = this.screen.layout.components.FirstOrDefault(c => c.tag == element.tag);
					this.element = element;

				}, highlighted: this.highlighted);

				if (GUI.Button(rectCloseButton, string.Empty, Layout.styles.closeButton) == true) {
					
					FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>().SetEnabled();
					this.opened = false;
					
				}

			}

		}

		public override void OnFlowWindowScreenMenuGUI(FD.FlowWindow windowSource, GenericMenu menu) {

			menu.AddItem(new GUIContent("Components Editor..."), on: false, func: (object win) => {

				var window = win as FD.FlowWindow;
				var screen = window.GetScreen() as LayoutWindowType;
				if (screen != null) {

					FlowSystemEditorWindow.GetWindow<FlowSystemEditorWindow>().SetDisabled();
					//this.window = window;
					this.screen = screen;
					this.editor = Editor.CreateEditor(window.GetScreen()) as IPreviewEditor;
					
					this.component = null;
					this.hovered = null;
					this.element = null;
					this.listScrollPosition = Vector2.zero;
					var serializedObject = new SerializedObject(this.screen);
					var layout = serializedObject.FindProperty("layout");
					var components = layout.FindPropertyRelative("components");
					this.props.Clear();
					for (int i = 0; i < components.arraySize; ++i) {
						
						var component = components.GetArrayElementAtIndex(i);
						this.props.Add(component);
						
						this.screen.layout.components[i].OnComponentChanged(this.screen, this.screen.layout.components[i].component);
						
					}
					
					this.settingsScrollPosition = Vector2.zero;
					
					this.opened = true;

				}

			}, userData: windowSource);

		}

		//private GUIStyle layoutBoxStyle;
		/*public override void OnFlowWindowGUI(FD.FlowWindow window) {

			var data = FlowSystem.GetData();
			if (data == null) return;

			if (data.modeLayer == ModeLayer.Flow) {

				if (window.IsContainer() == true ||
				    window.IsSmall() == true ||
				    window.IsShowDefault() == true)
					return;

				var screen = window.GetScreen() as LayoutWindowType;
				if (screen != null) {

					GUILayout.BeginVertical();
					{
						if (this.layoutBoxStyle == null) {
							
							this.layoutBoxStyle = FlowSystemEditorWindow.defaultSkin.FindStyle("LayoutBox");
							
						}

						GUILayout.FlexibleSpace();

						GUILayout.BeginHorizontal();
						{
							GUILayout.FlexibleSpace();

							if (GUILayoutExt.LargeButton("Open Editor", 50f, 160f) == true) {

							}

							GUILayout.FlexibleSpace();
						}
						GUILayout.EndHorizontal();

						GUILayout.FlexibleSpace();

					}
					GUILayout.EndVertical();

				}

			}

		}*/

		public override bool InstallationNeeded() {

			return false;

		}

	}

}