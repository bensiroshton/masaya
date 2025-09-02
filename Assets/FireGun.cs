using Siroshton.Masaya.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Siroshton.Masaya.Weapon;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FireGun", story: "Fire [gun] and play [sound]", category: "Action", id: "d701710a3d3416d0d8ad2c9e9d753cba")]
public partial class FireGun : Action
{
    [SerializeReference] public BlackboardVariable<Gun> gun;
    [SerializeReference] public BlackboardVariable<AudioClip> sound;

    protected override Status OnStart()
    {
        gun.Value.Trigger();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

