using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System.Reflection;
using System.Timers;
using UnityEngine;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;
using ColossalFramework.Plugins;

namespace TreeMapper
{
	public class TreeMapperUIPanel: UIPanel
	{
        const int INPUT_OFFSET = 120;

        UILabel title;
		
		UITextField boundingBoxTextBox;
		UILabel boundingBoxLabel;
		
		UILabel informationLabel;

		UITextField xScaleTextBox;
		UILabel xScaleLabel;

		UITextField yScaleTextBox;
		UILabel yScaleLabel;

		UITextField xOffsetTextBox;
		UILabel xOffsetLabel;

		UITextField yOffsetTextBox;
		UILabel yOffsetLabel;
		
		UITextField randomnessTextBox;
		UILabel randomnessLabel;

		UITextField densityTextBox;
		UILabel densityLabel;

        UIDropDown treeDropdown;
        UILabel treeLabel;

        UILabel errorLabel;

		UIButton importButton;

        UICheckBox clearCheckbox;

        public event PropertyChangedEventHandler<bool> eventEnableCheckChanged;

        public override void Awake()
		{
			this.isInteractive = true;
			this.enabled = true;
			
			width = 500;
			
			title = AddUIComponent<UILabel>();
			
            boundingBoxTextBox = UIUtils.CreateTextField(this);
            boundingBoxLabel = AddUIComponent<UILabel>();		
			
			informationLabel = AddUIComponent<UILabel>();
			
			randomnessTextBox = UIUtils.CreateTextField(this);
            randomnessLabel = AddUIComponent<UILabel>();

			xScaleTextBox = UIUtils.CreateTextField(this);
            xScaleLabel = AddUIComponent<UILabel> ();

			yScaleTextBox = UIUtils.CreateTextField(this);
            yScaleLabel = AddUIComponent<UILabel> ();

			xOffsetTextBox = UIUtils.CreateTextField(this);
            xOffsetLabel = AddUIComponent<UILabel> ();

			yOffsetTextBox = UIUtils.CreateTextField(this);
            yOffsetLabel = AddUIComponent<UILabel> ();

			densityTextBox = UIUtils.CreateTextField(this);
            densityLabel = AddUIComponent<UILabel>();

            treeDropdown = UIUtils.CreateDropDown(this);
            treeLabel = AddUIComponent<UILabel>();

            errorLabel = AddUIComponent<UILabel>();

            importButton = UIUtils.CreateButton(this);

            clearCheckbox = UIUtils.CreateCheckBox(this);

            base.Awake();
		}

		public override void Start()
		{
			base.Start();
			
			relativePosition = new Vector3(396, 58);
			backgroundSprite = "MenuPanel2";
			isInteractive = true;
			SetupControls();
		}
		
		public void SetupControls()
		{
			title.text = "Tree Mapper";
			title.relativePosition = new Vector3(15, 15);
			title.textScale = 0.9f;
			title.size = new Vector2(200, 30);
			var vertPadding = 30;
			var x = 15;
			var y = 50;
			
			x = 15;
			y += vertPadding;
			
			SetLabel(randomnessLabel, "Randomness", x, y);
			SetTextBox(randomnessTextBox, "40", "", x + INPUT_OFFSET, y);
			y += vertPadding;

			SetLabel(xScaleLabel, "X Scale", x, y);
			SetTextBox(xScaleTextBox, "16", "?", x + INPUT_OFFSET, y);
			y += vertPadding;

			SetLabel(yScaleLabel, "Y Scale", x, y);
			SetTextBox(yScaleTextBox, "20", "?", x + INPUT_OFFSET, y);
			y += vertPadding;

			SetLabel(xOffsetLabel, "X Offset", x, y);
			SetTextBox(xOffsetTextBox, "0", "?", x + INPUT_OFFSET, y);
			y += vertPadding;
			
			SetLabel(yOffsetLabel, "Y Offset", x, y);
			SetTextBox(yOffsetTextBox, "0", "?", x + INPUT_OFFSET, y);
			y += vertPadding;

			SetLabel(densityLabel, "Density", x, y);
			SetTextBox(densityTextBox, "1", "How many trees will be placed for each pixel of the tree map", x + INPUT_OFFSET, y);
			y += vertPadding;

			SetLabel(boundingBoxLabel, "Bounding Box", x, y);
			//Default is Frankfort, KY, which includes some interesting terrain and trees
			SetTextBox(boundingBoxTextBox, "-84.774950,38.264822,-84.980892,38.103126", "", x + INPUT_OFFSET, y);
            y += vertPadding;

            SetLabel(treeLabel, "Select a Tree:", x, y);
            InitializeTreeDropdown(x + INPUT_OFFSET, y);
            y += vertPadding;

            SetCheckbox(clearCheckbox, "Clear Existing Trees", "If true, Tree Mapper will remove all existing trees on the map before adding them.", false, x + INPUT_OFFSET, y);
            clearCheckbox.eventCheckChanged += (c, s) =>
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"clearCheckbox clicked: {s.ToString()}");
                eventEnableCheckChanged(this, s);
            };
            y += vertPadding;

            SetButton(importButton, "Import Trees", y);
			importButton.eventClick += importButton_eventClick;
			height = y + vertPadding + 6;
		}

        private void importButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
		{
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Button clicked");
            try
			{
				int density = int.Parse(densityTextBox.text);

				BoundingBox boundingBox = GetBoundingBox();

				TreeMapper treeMapper = new TreeMapper();

				treeMapper.XScale = double.Parse(xScaleTextBox.text);
				treeMapper.YScale = double.Parse(yScaleTextBox.text);

				treeMapper.XShift = double.Parse(xOffsetTextBox.text);
				treeMapper.YShift = double.Parse(yOffsetTextBox.text);

				treeMapper.Density = int.Parse(densityTextBox.text);
				treeMapper.Randomness = double.Parse(randomnessTextBox.text);

                treeMapper.SelectedTree = GetTreeByName(treeDropdown.selectedValue);

                if (clearCheckbox.isChecked)
                {
                    treeMapper.ClearTrees();
                }

                treeMapper.TreeMapperEvent += TreeMapperEvent;

                treeMapper.ImportTrees(boundingBox);
                //Thread thread = new Thread(() => treeMapper.ImportTrees(boundingBox));
                //thread.Start();
            }
			catch (Exception ex)
			{
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"Exception encountered: {ex.ToString()}");
                errorLabel.text = ex.ToString();
			}
		}

		private void TreeMapperEvent(object sender, TreeMapperArgs args)
		{
			errorLabel.text = args.Message;
		}

		private TreeInfo GetTreeByName(string name)
		{
			TreeCollection treeCollection= GameObject.FindObjectOfType<TreeCollection>();
			IList<TreeInfo> treeInfos = treeCollection.m_prefabs;

            return treeInfos.Where(tree => tree.name == name).FirstOrDefault();
		}

		private BoundingBox GetBoundingBox()
		{
			BoundingBox boundingBox = new BoundingBox ();

			IList<string> boundingBoxText = boundingBoxTextBox.text.Split (',');

			boundingBox.MinimumLatitude = double.Parse (boundingBoxText [0].Trim ());
			boundingBox.MinimumLongitude = double.Parse (boundingBoxText [1].Trim ());
			boundingBox.MaximumLatitude = double.Parse (boundingBoxText [2].Trim ());
			boundingBox.MaximumLongitude = double.Parse (boundingBoxText [3].Trim ());

			return boundingBox;
		}

		private void InitializeTreeDropdown(int x, int y)
		{
            TreeCollection treeCollection = GameObject.FindObjectOfType<TreeCollection>();
            IEnumerable<TreeInfo> treeInfos = treeCollection.m_prefabs;
            IEnumerable<string> treeNames = treeInfos.Select(tree => tree.name);

            InitializeDropdown(treeDropdown, treeNames, x, y);
        }
		
		private void SetButton(UIButton okButton, string p1, int y)
		{
			okButton.text = p1;
			okButton.size = new Vector2(260, 24);
			okButton.relativePosition = new Vector3((int)(width - okButton.size.x) / 2,y);
		}
		
		private void SetTextBox(UITextField scaleTextBox, string p, string tt, int x, int y)
		{
			scaleTextBox.relativePosition = new Vector3(x, y - 4);
			scaleTextBox.text = p;
            scaleTextBox.tooltip = tt;
            scaleTextBox.normalBgSprite = "TextFieldPanel";
            scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            scaleTextBox.focusedBgSprite = "TextFieldPanel";
            scaleTextBox.width = width - INPUT_OFFSET - 30;
        }

        private void SetCheckbox(UICheckBox checkBox, string label, string tt, bool v, int x, int y)
        {
            checkBox.label.text = label;
            checkBox.tooltip = tt;
            checkBox.isChecked = v;
            checkBox.relativePosition = new Vector3(x, y);
            checkBox.width = width - INPUT_OFFSET - 30;
        }

        private void InitializeDropdown(UIDropDown dropDown, IEnumerable<string> items, int x, int y)
		{
            dropDown.relativePosition = new Vector3(x, y - 4);
            dropDown.width = width - INPUT_OFFSET - 30;

            items.ForEach(item =>
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, $"Tree: {item}");
                dropDown.AddItem(item);
            });
		}
		
		private void SetLabel(UILabel pedestrianLabel, string p, int x, int y)
		{
			pedestrianLabel.relativePosition = new Vector3(x, y);
			pedestrianLabel.text = p;
			pedestrianLabel.textScale = 0.8f;
			pedestrianLabel.size = new Vector3(120,20);
		}
	}
}