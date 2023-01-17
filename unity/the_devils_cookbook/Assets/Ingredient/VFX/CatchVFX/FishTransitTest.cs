using UnityEngine;
using Random = UnityEngine.Random;

public class FishTransitTest : MonoBehaviour
{
    [SerializeField] private TDC.VFX.IngredientTransit _FishTransit;

    [SerializeField] private Vector3 _TargetA;
    [SerializeField] private Vector3 _TargetB;

    [SerializeField] private AnimationCurve _TransitSpinRadius;

    private int _NextTarget = 0;

    private void OnValidate()
    {
        _FishTransit.SpinRadiusCurve = _TransitSpinRadius;
    }

    private void TransitToTargets()
    {
        Color colour = new Color()
        {
            r = Random.value,
            g = Random.value,
            b = Random.value,
            a = 1
        };
        if (_NextTarget == 0)
        {
            _FishTransit.StartTransit(_TargetA, colour);
            _NextTarget = 1;
        }
        else
        {
            _FishTransit.StartTransit(_TargetB, colour);
            _NextTarget = 0;
        }
    }

    private void TransitTargetOneWay()
    {
        Color colour = new Color()
        {
            r = Random.value,
            g = Random.value,
            b = Random.value,
            a = 1
        };
        _FishTransit.transform.position = _TargetA;
        _FishTransit.StartTransit(_TargetB, colour);
        _NextTarget = 1;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _FishTransit.SpinRadiusCurve = _TransitSpinRadius;
        _FishTransit.TransitFinished += TransitTargetOneWay;
        TransitTargetOneWay();
    }
    
}
