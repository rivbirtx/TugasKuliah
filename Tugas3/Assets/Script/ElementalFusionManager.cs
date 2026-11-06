
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ElementalFusionManager : MonoBehaviour
{
    public enum Element
    {
        Fire,
        Earth,
        Water
    }

    [Header("Vuforia Image Targets")]
    public ObserverBehaviour fireTarget;
    public ObserverBehaviour earthTarget;
    public ObserverBehaviour waterTarget;

    [Header("Combination Prefabs")]
    public GameObject fireEarthResult;
    public GameObject earthFireResult;

    public GameObject earthWaterResult;
    public GameObject waterEarthResult;

    public GameObject waterFireResult;
    public GameObject fireWaterResult;

    [Header("Result Settings")]
    public Transform resultsParent;
    public float heightOffset = 0.08f;

    private readonly List<Element> detectionOrder =
        new List<Element>();

    private readonly Dictionary<ObserverBehaviour, Element>
        observerElements =
        new Dictionary<ObserverBehaviour, Element>();

    private readonly Dictionary<Element, ObserverBehaviour>
        elementObservers =
        new Dictionary<Element, ObserverBehaviour>();

    private GameObject activeResult;

    private Element activeFirst;
    private Element activeSecond;

    private bool hasActivePair = false;

    private bool initialized = false;

    void Start()
    {
        // Pastikan Results Parent sudah dihubungkan.
        if (resultsParent == null)
        {
            Debug.LogError(
                "Results Parent belum dihubungkan!"
            );

            return;
        }

        // Sembunyikan enam objek template.
        // Objek hasil Instantiate akan diaktifkan kembali.
        foreach (Transform child in resultsParent)
        {
            child.gameObject.SetActive(false);
        }

        // Daftarkan ketiga Image Target.
        RegisterTarget(fireTarget, Element.Fire);
        RegisterTarget(earthTarget, Element.Earth);
        RegisterTarget(waterTarget, Element.Water);

        initialized = true;

        RefreshFusion();

        Debug.Log("Elemental Fusion Manager siap.");
    }

    void RegisterTarget(
        ObserverBehaviour observer,
        Element element)
    {
        if (observer == null)
        {
            Debug.LogError(
                "Image Target belum dihubungkan: " + element
            );

            return;
        }

        observerElements[observer] = element;
        elementObservers[element] = observer;

        observer.OnTargetStatusChanged += OnTargetChanged;

        // Periksa status awal target.
        UpdateTracking(
            observer,
            observer.TargetStatus
        );
    }

    void OnTargetChanged(
        ObserverBehaviour observer,
        TargetStatus status)
    {
        UpdateTracking(observer, status);
    }

    void UpdateTracking(
        ObserverBehaviour observer,
        TargetStatus status)
    {
        if (!observerElements.TryGetValue(
            observer, out Element element))
        {
            return;
        }

        if (status.Status == Status.TRACKED)
        {
            if (!detectionOrder.Contains(element))
            {
                detectionOrder.Add(element);

                Debug.Log(
                    "Marker terdeteksi: " + element
                );
            }
        }
        else
        {
            if (detectionOrder.Remove(element))
            {
                Debug.Log(
                    "Marker hilang: " + element
                );
            }
        }

        if (initialized)
        {
            RefreshFusion();
        }
    }

    void RefreshFusion()
    {
        // Minimal dua marker harus terdeteksi.
        if (detectionOrder.Count < 2)
        {
            ClearFusion();
            return;
        }

        // Ambil dua marker pertama yang terdeteksi.
        Element first = detectionOrder[0];
        Element second = detectionOrder[1];

        // Jika pasangan tidak berubah,
        // jangan buat objek baru.
        if (hasActivePair &&
            activeFirst == first &&
            activeSecond == second &&
            activeResult != null)
        {
            return;
        }

        ClearFusion();

        // Ambil objek hasil berdasarkan urutan marker.
        GameObject template = GetCombination(
            first,
            second
        );

        if (template == null)
        {
            Debug.LogWarning(
                "Objek kombinasi belum dihubungkan: " +
                first + " + " + second
            );

            return;
        }

        // Pastikan kedua observer tersedia.
        if (!elementObservers.ContainsKey(first) ||
            !elementObservers.ContainsKey(second))
        {
            Debug.LogError(
                "Observer Image Target tidak ditemukan."
            );

            return;
        }

        activeFirst = first;
        activeSecond = second;
        hasActivePair = true;

        Vector3 position = GetMiddlePosition();

        // Buat salinan dari template.
        // Template boleh dalam keadaan inactive.
        activeResult = Instantiate(
            template,
            position,
            Quaternion.identity,
            resultsParent
        );

        // Aktifkan objek hasil kombinasi.
        activeResult.SetActive(true);

        Debug.Log(
            "FUSION BERHASIL: " +
            first + " + " + second
        );
    }

    GameObject GetCombination(
        Element first,
        Element second)
    {
        if (first == Element.Fire &&
            second == Element.Earth)
        {
            return fireEarthResult;
        }

        if (first == Element.Earth &&
            second == Element.Fire)
        {
            return earthFireResult;
        }

        if (first == Element.Earth &&
            second == Element.Water)
        {
            return earthWaterResult;
        }

        if (first == Element.Water &&
            second == Element.Earth)
        {
            return waterEarthResult;
        }

        if (first == Element.Water &&
            second == Element.Fire)
        {
            return waterFireResult;
        }

        if (first == Element.Fire &&
            second == Element.Water)
        {
            return fireWaterResult;
        }

        return null;
    }

    Vector3 GetMiddlePosition()
    {
        Transform first =
            elementObservers[detectionOrder[0]].transform;

        Transform second =
            elementObservers[detectionOrder[1]].transform;

        Vector3 middle =
            (first.position + second.position) / 2f;

        return middle +
            Vector3.up * heightOffset;
    }

    void Update()
    {
        // Ikuti posisi kedua marker.
        if (activeResult == null ||
            detectionOrder.Count < 2)
        {
            return;
        }

        activeResult.transform.position =
            GetMiddlePosition();
    }

    void ClearFusion()
    {
        if (activeResult != null)
        {
            Destroy(activeResult);
            activeResult = null;
        }

        hasActivePair = false;
    }

    void OnDestroy()
    {
        // Lepaskan event listener.
        foreach (var observer in observerElements.Keys)
        {
            if (observer != null)
            {
                observer.OnTargetStatusChanged -=
                    OnTargetChanged;
            }
        }
    }
}
