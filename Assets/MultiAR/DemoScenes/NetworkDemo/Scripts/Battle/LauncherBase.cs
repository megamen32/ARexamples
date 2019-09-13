using UnityEngine;

public abstract class LauncherBase : MonoBehaviour
{
    [HideInInspector] public ClientPlayerController projectileOwnerPeer;
    [HideInInspector] public Color                  projectileTrailColor;
    [HideInInspector] public int                    projectileOwnerPeerId => projectileOwnerPeer.playerControllerId;
    public abstract          void                   LaunchPrimary ();
    public abstract          void                   LaunchSecondary ();
}