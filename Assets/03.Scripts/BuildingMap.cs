using UnityEngine;

public class BuildingMap : MonoBehaviour
{
    [Header("Prefabs Settings")]
    public GameObject stonePrefab;
    public GameObject[,,] stones = new GameObject[16, 16, 32];

    [Header("Map Settings")]
    public float[,,] density = new float[16, 16, 16]; // 밀도 (아래 16칸만 사용)
    public Vector2[,,] dirVector = new Vector2[3, 3, 9]; // 방향 벡터
    public float densityThreshold = 0.5f; // 밀도 임계값

    void Start()
    {
        // 돌 소환
        for (int y = 0; y < 32; y++)        // 높이
        { for (int z = 0; z < 16; z++)      // 세로
            { for (int x = 0; x < 16; x++)  // 가로
                {
                    stones[x, y, z] = Instantiate(stonePrefab, new Vector3(x, y, z), Quaternion.identity);
                    stones[x, y, z].transform.parent = transform;
                }
            }
        }
    }

    void Update()
    {
        // 밀도 설정
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetDirVector();
            SetDensity();
        }
        // 돌 상태 변경
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StoneStateChanger();
        }
    }

    void SetDirVector()
    {
        for (int h = 0; h < 3; h++)
        { for (int y = 0; y < 3; y++)
            { for (int x = 0; x < 3; x++)
                {
                    dirVector[x, y, h] = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                }
            }
        }
    }

    void SetDensity()
    {
        for (int h = 0; h < 16; h++)
        { 
            for (int y = 0; y < 16; y++)
            { 
                for (int x = 0; x < 16; x++)
                {
                    density[x, y, h] = GetDot(x, y, h);
                }
            }
        }
    }

    float GetDot(int x, int y, int h)
    {
        // 8개의 점에서의 밀도 "총합"
        float sum = 0f;

        for (int h__ = 0; h__ <= 1; h__++)
        { for (int y__ = 0; y__ <= 1; y__++)
            { for (int x__ = 0; x__ <= 1; x__++)
                {
                    // 사용되는 방향 벡터
                    int x_ = x/4+x__, y_ = y/4+y__, h_ = h/4+h__;
                    Vector2 vec1 = dirVector[x_, y_, h_];

                    // 한 지점과 청크의 끝 사이의 벡터
                    Vector2 vec2 = new Vector2(x-(x_*4.5f-0.5f), y-(y_*4.5f-0.5f));

                    // 밀도 = 두 벡터의 내적
                    sum += Mathf.Clamp01(Vector2.Dot(vec1, vec2) + 0.5f);
                }
            }
        }

        // 8개의 점에서의 "평균" 밀도 반환
        return Mathf.Clamp01(sum / 8f);
    }

    void StoneStateChanger()
    {
        for (int z = 0; z < 32; z++)
        { for (int y = 0; y < 16; y++)
            { for (int x = 0; x < 16; x++)
                {
                    if (density[x, y, z] > densityThreshold)
                    {
                        stones[x, y, z].SetActive(true);
                    }
                    else
                    {
                        stones[x, y, z].SetActive(false);
                    }
                }
            }
        }
    }
}
