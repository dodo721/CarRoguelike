using UnityEngine;

[ExecuteAlways]
public class WorldClickable : MonoBehaviour {

    public WorldBoundIcon partner;
    public WorldBoundIcon iconPrefab;
    public RectTransform spawnParent;
    public Canvas renderTo;

    void Start () {
        if (Application.isPlaying) {
            if (partner == null && iconPrefab != null) {
                WorldBoundIcon icon = Instantiate(
                    iconPrefab.gameObject,
                    iconPrefab.transform.position,
                    iconPrefab.transform.rotation,
                    spawnParent.transform
                    ).GetComponent<WorldBoundIcon>();
                icon.BindToWorldObject(gameObject);
            } else if (partner != null) {
                partner.BindToWorldObject(gameObject);
            }
        }
    }

#if UNITY_EDITOR
    void Update () {
        if (!UnityEditor.EditorApplication.isPlaying && partner != null) {
            Vector3 target = transform.position + (Vector3.up * 4) + partner.followOffset;
            partner.GetComponent<RectTransform>().position = target;
        }
    }
#endif

}