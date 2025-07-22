using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineCamera playerCam;
    public CinemachineCamera summonCam;
    public float summonDuration = 2f;

    public void SummonFocus()
    {
        //Move camera to summon
        playerCam.Priority = 10;
        summonCam.Priority = 20;

        StartCoroutine(ReturnToPlayerAfterDelay());
    }

    private IEnumerator ReturnToPlayerAfterDelay()
    {
        yield return new WaitForSeconds(summonDuration);

        //Turn back camera to player
        summonCam.Priority = 10;
        playerCam.Priority = 20;
    }
}
