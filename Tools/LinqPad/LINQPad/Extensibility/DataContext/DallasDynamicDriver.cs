namespace LINQPad.Extensibility.DataContext
{
    using LINQPad.UI;
    using System;
    using System.Windows.Forms;

    internal class DallasDynamicDriver : AstoriaDynamicDriver
    {
        public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
        {
            return (c1.DatabaseInfo.Server == c2.DatabaseInfo.Server);
        }

        internal override string GetImageKey(IConnectionInfo r)
        {
            return "Dallas";
        }

        public override void InitializeContext(IConnectionInfo r, object context, QueryExecutionManager executionManager)
        {
            AstoriaHelper.InitializeContext(r, context, true);
        }

        public override bool ShowConnectionDialog(IConnectionInfo repository, bool isNewRepository)
        {
            using (DallasCxForm form = new DallasCxForm(repository))
            {
                return (form.ShowDialog() == DialogResult.OK);
            }
        }

        internal override string InternalID
        {
            get
            {
                return "DallasAuto";
            }
        }

        internal override int InternalSortOrder
        {
            get
            {
                return 40;
            }
        }

        public override string Name
        {
            get
            {
                return "Microsoft DataMarket Service";
            }
        }
    }
}

