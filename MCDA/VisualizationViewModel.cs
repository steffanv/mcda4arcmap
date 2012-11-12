using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using ESRI.ArcGIS.Carto;
using System.ComponentModel;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;

namespace MCDA.ViewModel
{
    class VisualizationViewModel : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private BindingList<MCDAWorkspaceContainer> _listOfMCDAWorkspaceContainer;
        private BindingList<IField> _listOfFields;
        private MCDAWorkspaceContainer _selectedMCDAWorkspaceContainer;

        private ToolFeatureClassLinkTracker _toolFeatureClassLinkTracker;
        private IClassBreaksRenderer _classBreaksRenderer;
        private IField _selectedIField;

        public VisualizationViewModel()
        {
            _toolFeatureClassLinkTracker = ToolFeatureClassLinkTracker.Instance;

            _listOfMCDAWorkspaceContainer = new BindingList<MCDAWorkspaceContainer>(_toolFeatureClassLinkTracker.GetAllMCDAWorkspaceContainerFromShadowWorkspace());

            _toolFeatureClassLinkTracker.PropertyChanged += new PropertyChangedEventHandler(_toolFeatureClassLinkTracker_PropertyChanged);
        }

        void _toolFeatureClassLinkTracker_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Notify(() => MCDAWorkspaceContainer);
        }

        public BindingList<MCDAWorkspaceContainer> MCDAWorkspaceContainer
        {
            get { return _listOfMCDAWorkspaceContainer; }
            set {

                X();

                _listOfMCDAWorkspaceContainer = value;
            }
        }

        public BindingList<IField> Fields
        {
            get { return _listOfFields; }
            set { _listOfFields = value; }
        }

        public MCDAWorkspaceContainer SelectedMCDAWorkspaceContainer
        {
            get { return _selectedMCDAWorkspaceContainer; }
            set { _selectedMCDAWorkspaceContainer = value; }
        }

        public IField SelectedIField
        {
            get { return _selectedIField; }
            set { _selectedIField = value; }
        }

        private void X()
        {
            _listOfFields = new BindingList<IField>( MCDAExtension.GetListOfFieldsFromFeatureClass(_selectedMCDAWorkspaceContainer.FeatureClass));

            PropertyChanged.Notify(() => SelectedMCDAWorkspaceContainer);
        }
	}
}
