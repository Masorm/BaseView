using System.Collections.Generic;
using UnityEngine;

namespace BaseView.Plugins.Runtime
{
    public class LinkView : MonoBehaviour, ILinkView
    {
        [SerializeField] private List<LinkObject> linkObjects;
        public List<LinkObject> LinkObjects => linkObjects;
    }
}
