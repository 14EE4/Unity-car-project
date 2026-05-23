using System;
using System.Reflection;
using UnityEngine;
using TMPro;

public class SpeedAndGearUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI rpmText;

    [Header("Vehicle Source (optional)")]
    public Rigidbody targetRigidbody;
    public MonoBehaviour vehicleScript; // assign your car controller if available
    public string speedFieldName = "CurrentSpeed";
    public string gearFieldName = "CurrentGear";
    public string rpmFieldName = "CurrentRPM";
    public string rpmWarningFieldName = "IsRpmWarning";

    [Header("Formatting")]
    public float speedMultiplier = 3.6f; // m/s -> km/h
    public string speedFormat = "0"; // string format for speed
    public string gearFormat = "G{0}"; // e.g. G1, G2
    public string rpmFormat = "0";
    public string speedPrefix = "Speed: ";
    public string speedSuffix = " km/h";
    public string gearPrefix = "Gear: ";
    public string rpmPrefix = "RPM: ";

    [Header("RPM Warning")]
    public Color rpmNormalColor = Color.white;
    public Color rpmWarningColor = new Color(1f, 0.35f, 0.2f);

    void Update()
    {
        // Only auto-update from components if a source is assigned.
        bool useAuto = (targetRigidbody != null) || (vehicleScript != null);
        if (!useAuto) return;

        float speed = 0f;
        int gear = 0;
        float rpm = 0f;
        bool rpmWarning = false;

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

            var rf = type.GetField(rpmFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (rf != null)
            {
                rpm = ToFloat(rf.GetValue(vehicleScript));
            }
            else
            {
                var rp = type.GetProperty(rpmFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (rp != null) rpm = ToFloat(rp.GetValue(vehicleScript));
            }

            var warnField = type.GetField(rpmWarningFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (warnField != null)
            {
                rpmWarning = ToBool(warnField.GetValue(vehicleScript));
            }
            else
            {
                var warnProp = type.GetProperty(rpmWarningFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (warnProp != null) rpmWarning = ToBool(warnProp.GetValue(vehicleScript));
            }
        }

        if (speedText != null) speedText.text = string.Format("{0}{1}{2}", speedPrefix, Mathf.RoundToInt(speed).ToString(speedFormat), speedSuffix);
        if (gearText != null) gearText.text = string.Format("{0}{1}", gearPrefix, FormatGear(gear));
        if (rpmText != null)
        {
            rpmText.text = string.Format("{0}{1}", rpmPrefix, Mathf.RoundToInt(rpm).ToString(rpmFormat));
            rpmText.color = rpmWarning ? rpmWarningColor : rpmNormalColor;
        }
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

    bool ToBool(object o)
    {
        if (o == null) return false;
        if (o is bool) return (bool)o;
        bool res;
        if (bool.TryParse(o.ToString(), out res)) return res;
        return false;
    }

    // Optional: allow other scripts to push values directly
    public void SetSpeed(float s)
    {
        if (speedText != null) speedText.text = string.Format("{0}{1}{2}", speedPrefix, Mathf.RoundToInt(s).ToString(speedFormat), speedSuffix);
    }

    public void SetGear(int g)
    {
        if (gearText != null) gearText.text = string.Format("{0}{1}", gearPrefix, FormatGear(g));
    }

    public void SetRPM(float rpm, bool warning)
    {
        if (rpmText != null)
        {
            rpmText.text = string.Format("{0}{1}", rpmPrefix, Mathf.RoundToInt(rpm).ToString(rpmFormat));
            rpmText.color = warning ? rpmWarningColor : rpmNormalColor;
        }
    }

    string FormatGear(int g)
    {
        if (g < 0) return "R";
        if (g == 0) return "N";
        return g.ToString();
    }
}
