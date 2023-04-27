using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TeamInterface;

public class csBluePrintBase : csDropItemBase
{
    [SerializeField]
    Enum_BluePrintType bpType;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

}
