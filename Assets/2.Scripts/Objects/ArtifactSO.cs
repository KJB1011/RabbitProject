using UnityEngine;
using static Defines;

/// <summary>
/// 아티팩트 데이터 에셋.
/// </summary>
[CreateAssetMenu(menuName = "Artifact/Artifact Data", fileName = "Artifact")]
public class ArtifactSO : ScriptableObject
{
    public ArtifactId id;
    public string artifactName;
    [TextArea] public string description;
    public Sprite icon;
    public int price; // 상점 구매 가격
}