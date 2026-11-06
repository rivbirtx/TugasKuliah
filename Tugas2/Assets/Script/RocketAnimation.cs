
using UnityEngine;
using Vuforia;
using System.Collections;

public class RocketAnimation : MonoBehaviour
{
    [Header("Vuforia Image Targets")]
    public ObserverBehaviour rocketTarget;
    public ObserverBehaviour moonTarget;

    [Header("3D Objects")]
    public GameObject rocket;
    public Transform moon;

    [Header("Animation Settings")]
    public float speed = 0.35f;
    public float pauseDuration = 0.5f;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private Coroutine flightCoroutine;

    void Start()
    {
        initialLocalPosition = rocket.transform.localPosition;
        initialLocalRotation = rocket.transform.localRotation;

        rocket.SetActive(false);
    }

    void Update()
    {
        bool rocketDetected = IsTracked(rocketTarget);
        bool moonDetected = IsTracked(moonTarget);

        // Jika gambar roket hilang
        if (!rocketDetected)
        {
            StopFlight();
            rocket.SetActive(false);
            return;
        }

        // Jika gambar roket terdeteksi
        if (!rocket.activeSelf)
        {
            rocket.transform.localPosition = initialLocalPosition;
            rocket.transform.localRotation = initialLocalRotation;
            rocket.SetActive(true);
        }

        // Jika gambar bulan hilang, roket kembali ke awal
        if (!moonDetected)
        {
            StopFlight();
            return;
        }

        // Mulai animasi jika belum berjalan
        if (flightCoroutine == null)
        {
            flightCoroutine = StartCoroutine(FlyRepeatedly());
        }
    }

    bool IsTracked(ObserverBehaviour target)
    {
        return target != null &&
            (target.TargetStatus.Status == Status.TRACKED ||
             target.TargetStatus.Status == Status.EXTENDED_TRACKED);
    }

    IEnumerator FlyRepeatedly()
    {
        while (IsTracked(rocketTarget) && IsTracked(moonTarget))
        {
            // Ambil posisi bulan
            Vector3 destination = moon.position;

            // Terbang menuju bulan
            while (Vector3.Distance(
                rocket.transform.position,
                destination) > 0.03f)
            {
                if (!IsTracked(rocketTarget) ||
                    !IsTracked(moonTarget))
                    break;

                rocket.transform.position = Vector3.MoveTowards(
                    rocket.transform.position,
                    destination,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            if (!IsTracked(rocketTarget) ||
                !IsTracked(moonTarget))
                break;

            // Berhenti sebentar ketika mencapai bulan
            yield return new WaitForSeconds(pauseDuration);

            // Kembali ke posisi awal pada Image Target roket
            rocket.transform.localPosition = initialLocalPosition;
            rocket.transform.localRotation = initialLocalRotation;

            // Jeda sebelum terbang kembali
            yield return new WaitForSeconds(pauseDuration);
        }

        // Kembalikan posisi ketika tracking terputus
        rocket.transform.localPosition = initialLocalPosition;
        rocket.transform.localRotation = initialLocalRotation;

        flightCoroutine = null;
    }

    void StopFlight()
    {
        if (flightCoroutine != null)
        {
            StopCoroutine(flightCoroutine);
            flightCoroutine = null;
        }

        rocket.transform.localPosition = initialLocalPosition;
        rocket.transform.localRotation = initialLocalRotation;
    }
}
