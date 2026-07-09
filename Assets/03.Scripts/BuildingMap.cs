using UnityEngine;

public class BuildingMap : MonoBehaviour
{
    [Header("Prefabs Settings")]
    public GameObject stonePrefab;
    private GameObject[,,] stones = new GameObject[16, 32, 16]; // 유니티 관례상 [X, Y(높이), Z]로 매칭

    [Header("Map Settings")]
    // 아래 16칸만 밀도를 사용하므로 Y축은 16 크기로 선언
    public float[,,] density = new float[16, 16, 16]; 
    
    // 16칸 공간을 4칸 단위 격자로 나누면 꼭짓점은 0, 4, 8, 12, 16으로 총 5개가 필요함 (3D이므로 Vector3)
    public Vector3[,,] dirVectors = new Vector3[5, 5, 5]; 
    public float densityThreshold = 0.5f; // 밀도 임계값
    public float[,] heightMap = new float[16, 16]; // 지상 높이 맵 (2D)

    void Start()
    {
        // 돌 미리 소환 (X:16, Y:32, Z:16)
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    stones[x, y, z] = Instantiate(stonePrefab, new Vector3(x, y, z), Quaternion.identity);
                    stones[x, y, z].transform.parent = transform;
                }
            }
        }
    }

    void Update()
    {
        // 1번 누르면 방향 벡터 세팅 및 밀도 계산
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetDirVector();
            SetDensity();
            Debug.Log("밀도 계산 완료 (1번)");
        }
        
        // 2번 누르면 돌 상태 변경
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StoneStateChanger();
            GenerateGround();
            Debug.Log("맵에 노이즈 적용 완료 (2번)");
        }
    }

    void SetDirVector()
    {
        // 5x5x5 격자 꼭짓점에 무작위 3D 방향 벡터 할당
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = 0; z < 5; z++)
                {
                    dirVectors[x, y, z] = Random.onUnitSphere; // 3D 정규화된 구면 벡터
                }
            }
        }
    }

    void SetDensity()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                for (int z = 0; z < 16; z++)
                {
                    density[x, y, z] = GetNoise3D(x, y, z);
                }
            }
        }
    }

    // 부드러운 보간을 위한 Smoothstep 함수 (펄린 노이즈의 핵심)
    float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    float GetNoise3D(int x, int y, int z)
    {
        // 격자 크기인 4로 나누어 현재 속한 격자 인덱스 구하기
        int x0 = x / 4; int x1 = x0 + 1;
        int y0 = y / 4; int y1 = y0 + 1;
        int z0 = z / 4; int z1 = z0 + 1;

        // 격자 내부에서의 상대적 위치 (0.0 ~ 1.0)
        float tx = (x % 4) / 4f;
        float ty = (y % 4) / 4f;
        float tz = (z % 4) / 4f;

        // 8개 꼭짓점에서의 내적값 계산
        float d000 = Vector3.Dot(dirVectors[x0, y0, z0], new Vector3(tx, ty, tz));
        float d100 = Vector3.Dot(dirVectors[x1, y0, z0], new Vector3(tx - 1, ty, tz));
        float d010 = Vector3.Dot(dirVectors[x0, y1, z0], new Vector3(tx, ty - 1, tz));
        float d110 = Vector3.Dot(dirVectors[x1, y1, z0], new Vector3(tx - 1, ty - 1, tz));
        float d001 = Vector3.Dot(dirVectors[x0, y0, z1], new Vector3(tx, ty, tz - 1));
        float d101 = Vector3.Dot(dirVectors[x1, y0, z1], new Vector3(tx - 1, ty, tz - 1));
        float d011 = Vector3.Dot(dirVectors[x0, y1, z1], new Vector3(tx, ty - 1, tz - 1));
        float d111 = Vector3.Dot(dirVectors[x1, y1, z1], new Vector3(tx - 1, ty - 1, tz - 1));

        // 보간을 위한 페이드 값 계산
        float u = Fade(tx);
        float v = Fade(ty);
        float w = Fade(tz);

        // 8개 점을 축 방향으로 차례대로 선언적 보간 (Lerp)
        float x00 = Mathf.Lerp(d000, d100, u);
        float x10 = Mathf.Lerp(d010, d110, u);
        float x01 = Mathf.Lerp(d001, d101, u);
        float x11 = Mathf.Lerp(d011, d111, u);

        float r0 = Mathf.Lerp(x00, x10, v);
        float r1 = Mathf.Lerp(x01, x11, v);

        float value = Mathf.Lerp(r0, r1, w);

        // 내적 결과인 -1~1 사이의 값을 0~1 범위로 매핑하여 반환
        return (value + 1f) / 2f;
    }

    void StoneStateChanger()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 32; y++) // 전체 높이 32 돌기
            {
                for (int z = 0; z < 16; z++)
                {
                    // 아래 16칸은 계산된 밀도 데이터 사용
                    if (y < 16)
                    {
                        if (density[x, y, z] > densityThreshold)
                            stones[x, y, z].SetActive(true);
                        else
                            stones[x, y, z].SetActive(false); // 밀도가 낮으면 파내서 동굴 생성
                    }
                    else if (y >= 28)
                    {
                        // 위쪽 4칸(28~31)은 밀도 데이터가 없으므로 무조건 비활성화 (지상 공간)
                        // GenerateGround();
                    }
                    else
                    {
                        // 중간 4칸(28~31)은 지상 공간과 연결되도록 무조건 활성화
                        stones[x, y, z].SetActive(true);
                    }
                }
            }
        }
    }

    float GetNoise2D(int x, int z)
    {
        // 격자 크기인 4로 나누어 현재 속한 격자 인덱스 구하기
        int x0 = x / 4; int x1 = x0 + 1;
        int z0 = z / 4; int z1 = z0 + 1;

        // 격자 내부에서의 상대적 위치 (0.0 ~ 1.0)
        float tx = (x % 4) / 4f;
        float tz = (z % 4) / 4f;

        // 8개 꼭짓점에서의 내적값 계산
        float d000 = Vector3.Dot(dirVectors[x0, 4, z0], new Vector3(tx, 0.5f, tz));
        float d100 = Vector3.Dot(dirVectors[x1, 4, z0], new Vector3(tx - 1, 0.5f, tz));
        float d010 = Vector3.Dot(dirVectors[x0, 4, z1], new Vector3(tx, -0.5f, tz));
        float d110 = Vector3.Dot(dirVectors[x1, 4, z1], new Vector3(tx - 1, -0.5f, tz));
        float d001 = Vector3.Dot(dirVectors[x0, 4, z1], new Vector3(tx, 0.5f, tz - 1));
        float d101 = Vector3.Dot(dirVectors[x1, 4, z1], new Vector3(tx - 1, 0.5f, tz - 1));
        float d011 = Vector3.Dot(dirVectors[x0, 4, z1], new Vector3(tx, -0.5f, tz - 1));
        float d111 = Vector3.Dot(dirVectors[x1, 4, z1], new Vector3(tx - 1, -0.5f, tz - 1));

        // 보간을 위한 페이드 값 계산
        float u = Fade(tx);
        float v = Fade(0.25f);
        float w = Fade(tz);

        // 8개 점을 축 방향으로 차례대로 선언적 보간 (Lerp)
        float x00 = Mathf.Lerp(d000, d100, u);
        float x10 = Mathf.Lerp(d010, d110, u);
        float x01 = Mathf.Lerp(d001, d101, u);
        float x11 = Mathf.Lerp(d011, d111, u);

        float r0 = Mathf.Lerp(x00, x10, v);
        float r1 = Mathf.Lerp(x01, x11, v);

        float value = Mathf.Lerp(r0, r1, w);

        // 내적 결과인 -1~1 사이의 값을 0~1 범위로 매핑하여 반환
        return (value + 1f) / 2f;
    }

    void GenerateHeightMap()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                // 낮은 주파수로 부드러운 지형 생성
                heightMap[x, z] = GetNoise2D(x, z) * 4f;
            }
        }
    }

    // 지상(위 4칸) 펄린 노이즈 2D로 생성
    void GenerateGround()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                for (int y = 28; y < 32; y++) // 위쪽 4칸(28~31)만 지상 공간
                {
                    Debug.Log($"HeightMap[{x}, {z}] = {heightMap[x, z]}");
                    if (y <= 28 + heightMap[x, z]) // 지상 높이 맵에 따라 돌 활성화
                        stones[x, y, z].SetActive(true);
                    else
                        stones[x, y, z].SetActive(false);
                }
            }
        }
    }
}