using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SpeedAndGearUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text speedText;
    public Text gearText;

    [Header("Vehicle Source (optional)")]
    public Rigidbody targetRigidbody;
    public MonoBehaviour vehicleScript; // assign your car controller if available
    public string speedFieldName = "CurrentSpeed";
    public string gearFieldName = "CurrentGear";

    [Header("Formatting")]
    public float speedMultiplier = 3.6f; // m/s -> km/h
    public string speedFormat = "0"; // string format for speed
    public string gearFormat = "G{0}"; // e.g. G1, G2

    void Update()
    {
        float speed = 0f;
        int gear = 0;

        if (targetRigidbody != null)
        {
            speed = targetRigidbody.linearVelocity.magnitude * speedMultiplier;
        }
        else if (vehicleScript != null)
        {
            var type = vehicleScript.GetType();

            // try field then property for speed
            var f = type.GetField(speedFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (f != null)
            {
                var val = f.GetValue(vehicleScript);
                speed = ToFloat(val);
            }
            else
            {
                var p = type.GetProperty(speedFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (p != null) speed = ToFloat(p.GetValue(vehicleScript));
            }

            // try field then property for gear
            var gf = type.GetField(gearFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (gf != null)
            {
                var val = gf.GetValue(vehicleScript);
                gear = ToInt(val);
            }
            else
            {
                var gp = type.GetProperty(gearFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (gp != null) gear = ToInt(gp.GetValue(vehicleScript));
            }
        }

        if (speedText != null) speedText.text = Mathf.RoundToInt(speed).ToString(speedFormat);
        if (gearText != null) gearText.text = string.Format(gearFormat, gear);
    }

    float ToFloat(object o)
    {
        if (o == null) return 0f;
        if (o is float) return (float)o;
        if (o is double) return (float)(double)o;
        if (o is int) return (int)o;
        if (o is long) return (long)o;
        float res;
        if (float.TryParse(o.ToString(), out res)) return res;
        return 0f;
    }

    int ToInt(object o)
    {
        if (o == null) return 0;
        if (o is int) return (int)o;
        if (o is long) return (int)(long)o;
        if (o is float) return Mathf.RoundToInt((float)o);
        int res;
        if (int.TryParse(o.ToString(), out res)) return res;
        return 0;
    }

    // Optional: allow other scripts to push values directly
    public void SetSpeed(float s)
    {
        if (speedText != null) speedText.text = Mathf.RoundToInt(s).ToString(speedFormat);
    }

    public void SetGear(int g)
    {
        if (gearText != null) gearText.text = string.Format(gearFormat, g);
    }
}
