﻿using HueLib2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinHue3.Addons.Model;

namespace WinHue3.Addons.ViewModel
{
    public class AlertCreatorViewModel : ValidatableBindableBase
    {
        private AlertCreatorModel _alertCreatorModel;
        private ObservableCollection<Criteria> _criterias;
        private Body _action;

        public AlertCreatorModel AlertCreatorModel
        {
            get
            {
                return _alertCreatorModel;
            }

            set
            {
                SetProperty(ref _alertCreatorModel,value);
            }
        }

        public AlertCreatorViewModel()
        {
            _alertCreatorModel = new AlertCreatorModel();
        }


    }
}