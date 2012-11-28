using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Entity;
using System.Data;

namespace MCDA.Model
{
    public sealed class OWATool : AbstractToolTemplate
    {
        private DataTable _workingDataTable, _backupDataTable;
        private ToolParameterContainer _toolParameterContainer;
        private TransformationStrategy _transformationStrategy;

        private string _defaultResultColumnName = "OWAResult";

        public override DataTable Data
        {
            get { return _workingDataTable.Copy(); }
        }

        public override ToolParameterContainer ToolParameterContainer
        {
            get { return _toolParameterContainer; }
            set { _toolParameterContainer = value; }
        }

        public override TransformationStrategy TransformationStrategy
        {
            get { return _transformationStrategy; }
            set { _transformationStrategy = value; }
        }

        protected override void PerformAlgorithm()
        {
            throw new NotImplementedException();
        }

        protected override void PerformScaling()
        {
            throw new NotImplementedException();
        }

        public override string DefaultResultColumnName
        {
            get { return _defaultResultColumnName; }
            set { _defaultResultColumnName = value; }
        }

        public override string ToString()
        {
            return "OWA Tool";
        }
    }
}
