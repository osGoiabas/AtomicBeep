using UnityEngine;
using System.Collections;

namespace Giometric.UniSonic.Objects
{
    public class DamageTrigger : ObjectTriggerBase
    {
        private int quantoMachuca = 15;

        protected override Color32 gizmoColor { get { return new Color32(255, 16, 16, 64); } }

        protected override void OnPlayerEnterTrigger(PlayerMovement player)
        {
            base.OnPlayerEnterTrigger(player);
            if (!player.IsInvulnerable)
            {
                player.SetHitState(transform.position, quantoMachuca);
            }
        }
    }
}