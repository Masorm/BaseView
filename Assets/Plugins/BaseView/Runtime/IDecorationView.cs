using System.Collections.Generic;

namespace BaseView.Plugins.Runtime
{
    public interface IDecorationView
    {
        List<DecorationTarget> DecorationList { get; }
    }
}
