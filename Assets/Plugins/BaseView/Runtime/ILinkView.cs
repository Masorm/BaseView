using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseView.Plugins.Runtime
{
    public interface ILinkView
    {
        List<LinkObject> LinkObjects { get; }
    }

    [Serializable]
    public class LinkObject
    {
        public int Position;
        public GameObject ClickableObject;
        public MeshRenderer LinkMarkObject;
    }
}
