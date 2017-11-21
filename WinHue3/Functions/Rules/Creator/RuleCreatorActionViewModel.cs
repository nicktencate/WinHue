﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinHue3.ExtensionMethods;
using WinHue3.Philips_Hue.BridgeObject;
using WinHue3.Philips_Hue.BridgeObject.BridgeObjects;
using WinHue3.Philips_Hue.Communication;
using WinHue3.Philips_Hue.HueObjects.Common;
using WinHue3.Philips_Hue.HueObjects.LightObject;
using WinHue3.Philips_Hue.HueObjects.RuleObject;
using WinHue3.Philips_Hue.HueObjects.SceneObject;
using WinHue3.Philips_Hue.HueObjects.ScheduleObject;
using WinHue3.Utils;
using Action = WinHue3.Philips_Hue.HueObjects.GroupObject.Action;
using WinHue3.Philips_Hue.HueObjects.NewSensorsObject;

namespace WinHue3.Functions.Rules.Creator
{
    public class RuleCreatorActionViewModel : ValidatableBindableBase
    {
        private ObservableCollection<RuleAction> _listRuleActions;
        private RuleAction _selectedRuleAction;
        private ObservableCollection<HuePropertyTreeViewItem> _listBridgeResources;
        private ObservableCollection<HuePropertyTreeViewItem> _listHueObjectProperties;
        private HuePropertyTreeViewItem _selectedHueObject;
        private HueObject _actionProperties;
        private HueObject _currentProperties;
        private HuePropertyTreeViewItem _selectedHueProperty;
        private bool _canChangeHueObject;
        private string _propertyValue;

        public RuleCreatorActionViewModel()
        {
            _listRuleActions = new ObservableCollection<RuleAction>();
            _listBridgeResources = new ObservableCollection<HuePropertyTreeViewItem>();
            _listHueObjectProperties = new ObservableCollection<HuePropertyTreeViewItem>();
            _currentProperties = new HueObject();
            _propertyValue = string.Empty;
        }

        public void Initialize(DataStore ds)
        {
            _listBridgeResources.Add(TreeViewHelper.BuildPropertiesTreeFromDataStore(ds));

        }

        public ICommand AddActionCommand => new RelayCommand(param => AddAction(), (param) => CanAddAction());
        public ICommand RemoveRuleActionCommand => new RelayCommand(param => RemoveRuleAction());
        public ICommand SelectRuleActionCommand => new RelayCommand(param => SelectRuleAction());

        private void SelectHueObject()
        {
            if (SelectedHueObject == null) return;
            if (SelectedHueObject.Items.Count > 0) return;
            ListHueObjectProperties.Clear();
            
        }

        private void SelectRuleAction()
        {
            if (SelectedRuleAction == null) return;
            SelectedHueObject = ParseRuleForObject(SelectedRuleAction.address.ToString(),ListBridgeResources[0]);
        }

        private HuePropertyTreeViewItem ParseRuleForObject(string address, HuePropertyTreeViewItem rtvi)
        {
            
            if (rtvi.Tag.ToString().Equals(address)) return rtvi;
            if (rtvi.Items.Count == 0) return null;

            foreach (HuePropertyTreeViewItem r in rtvi.Items)
            {
                HuePropertyTreeViewItem nrt = ParseRuleForObject(address, r);
                if (nrt != null) return nrt;
            }
            return null;
        }

        private void RemoveRuleAction()
        {
            ListRuleActions.Remove(SelectedRuleAction);
            SelectedRuleAction = null;
        }

        private bool CanAddAction()
        {
            if (ListRuleActions.Count > 7) return false;
            if (SelectedHueObject == null) return false;
            if (SelectedHueObject.Items.Count > 0) return false;
            if (string.IsNullOrEmpty(PropertyValue) || string.IsNullOrWhiteSpace(PropertyValue)) return false;
            if (Serializer.SerializeToJson(ActionProperties) == "{}") return false;
            return true;
        }

        private void AddAction()
        {
            DialogResult result = DialogResult.Yes;

            RuleAction ra = new RuleAction();
            HueAddress address = new HueAddress(SelectedHueObject.Tag.ToString());
            switch (address.objecttype)
            {
                case "lights":
                    address.property = "state";
                    break;
                case "groups":
                    address.property= "action";
                    break;
                case "schedules":
                    address.property = "command";
                    address.subprop = "body";
                    break;
                case "resourcelinks":
                    address.property = "links";
                    break;
                case "scenes":
                    address = new HueAddress($"/groups/0/action");
                    break;
                case "sensors":
                    address.property = "state";
                    break;
                default:
                    break;
            }

            if (ListRuleActions.Any(x => x.address == address))
            {
                result = MessageBox.Show(GlobalStrings.Rule_ActionAlreadyExists, GlobalStrings.Warning,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    ListRuleActions.Remove(ListRuleActions.FirstOrDefault(x => x.address == address));
            }

            if (result != DialogResult.Yes) return;
            ra.address = address;
            ra.method = "PUT";
            ra.body = _actionProperties != null ? Serializer.SerializeToJson(_actionProperties) : Serializer.SerializeToJson(new SceneBody() {scene = address.id});
            ListRuleActions.Add(ra);
        }

        public ObservableCollection<RuleAction> ListRuleActions
        {
            get => _listRuleActions;
            set => SetProperty(ref _listRuleActions,value);
        }

        public HuePropertyTreeViewItem SelectedHueObject
        {
            get => _selectedHueObject;
            set
            {
                SetProperty(ref _selectedHueObject, value);
                SelectHueObject();
            }
        }

        public HueObject ActionProperties
        {
            get => _actionProperties;
            set => SetProperty(ref _actionProperties,value);
        }

        public RuleAction SelectedRuleAction
        {
            get => _selectedRuleAction;
            set => SetProperty(ref _selectedRuleAction,value);
        }

        public ObservableCollection<HuePropertyTreeViewItem> ListBridgeResources
        {
            get => _listBridgeResources;
            set => SetProperty(ref _listBridgeResources,value);
        }

        public ObservableCollection<HuePropertyTreeViewItem> ListHueObjectProperties
        {
            get { return _listHueObjectProperties; }
            set { SetProperty(ref _listHueObjectProperties, value); }
        }

        public bool CanChangeHueObject
        {
            get
            {
                if (!_currentProperties.Any()) return true;
                return _canChangeHueObject;
                
            }
            
        }

        public string PropertyValue
        {
            get { return _propertyValue; }
            set { SetProperty(ref _propertyValue,value); }
        }
    }
}