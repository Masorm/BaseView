using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseView.Plugins.Runtime
{
    public class DecorationView : MonoBehaviour
    {
        [SerializeField] private List<DecorationTarget> decorationList;
        public List<DecorationTarget> DecorationList => decorationList;
    }

    [Serializable]
    public class DecorationTarget
    {
        [SerializeField] private int position;
        [SerializeField] private MeshRenderer targetMesh;
        public int Position => position;
        public MeshRenderer TargetMesh => targetMesh;
    }
}
