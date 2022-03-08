using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ultramath : MonoBehaviour
{
    // this ALWAYS comes up and I don't understand why it's not in the language
    // https://stackoverflow.com/questions/2691025/mathematical-modulus-in-c-sharp
    public static int MathMod(int a, int b) {
        return (Mathf.Abs(a * b) + a) % b;
    }
}
