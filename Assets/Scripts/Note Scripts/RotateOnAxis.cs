
using UnityEngine;

public class RotateOnAxis : MonoBehaviour
{
    public int axis = 1;

    public float multiplier = 1f;

    private void Update()
    {
        if (axis == 1)
        {
            base.transform.Rotate(0f, (0f - Time.deltaTime) * 60f * multiplier, 0f);
        }
        else if (axis == 0)
        {
            base.transform.Rotate((0f - Time.deltaTime) * 60f * multiplier, 0f, 0f);
        }
        else
        {
            base.transform.Rotate(0f, 0f, (0f - Time.deltaTime) * 60f * multiplier);
        }
    }
}
