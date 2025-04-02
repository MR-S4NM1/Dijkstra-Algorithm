using UnityEngine;

public class Recursivity : MonoBehaviour
{
    public int recursivityValue = 8;

    void Start()
    {
        DeacummulativeRecursivityMethod(recursivityValue);
    }

    protected int DeacummulativeRecursivityMethod(int value)
    {
        if(value <= 0)
        {
            Debug.Log($"{this.name} - {gameObject.name} - " +
                $"DeacummulativeRecursivityMethod(int): BREAK condition fulfilled :P {value}");
            return 0;
        }
        Debug.Log($"{this.name} - {gameObject.name} - " +
            $"DeacummulativeRecursivityMethod(int): Recursive condition fulfilled :P {value}");
        value--;
        return DeacummulativeRecursivityMethod(value);
    }
}
