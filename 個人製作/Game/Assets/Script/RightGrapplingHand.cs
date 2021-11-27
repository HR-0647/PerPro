using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

/**
 * Sample component that shows how to use Obi Rope to create a grappling hook for a 2.5D game.
 * 95% of the code is the grappling hook logic (user input, scene raycasting, launching, attaching the hook, etc) and parameter setup,
 * to show how to use Obi completely at runtime. This might not be practical for real-world scenarios,
 * but illustrates how to do it.
 *
 * Note that the choice of using actual rope simulation for grapple dynamics is debatable. Usually
 * a simple spring works better both in terms of performance and controllability. 
 *
 * If complex interaction is required with the scene, a purely geometry-based approach (ala Worms ninja rope) can
 * be the right choice under certain circumstances.
 * 
 * Obi Ropeを使って2.5Dゲーム用のグラップリングフックを作成する方法を示すサンプルコンポーネントです。
   コードの95％はグラップリングフックのロジック（ユーザーの入力、シーンのレイキャスティング、発射、フックの取り付けなど）とパラメータの設定です。
   ランタイムで完全に帯を使用する方法を示すためです。これは実際のシナリオでは実用的ではないかもしれません。
   しかし、その方法を説明しています。

   なお、グラップルダイナミクスに実際のロープシミュレーションを使うかどうかは議論の余地があります。通常は
   単純なバネの方が性能的にも制御性の面でも優れています。

   シーンとの複雑なインタラクションが必要な場合は、純粋にジオメトリベースのアプローチ（Worms ninja ropeなど）が適しています。
   (Worms ninja ropeのような)純粋なジオメトリベースのアプローチは
   状況によっては正しい選択となるでしょう。
 */
public class RightGrapplingHand : MonoBehaviour
{

    public ObiSolver solver;
    public ObiCollider RightHand;
    public float hookExtendRetractSpeed = 2f;
    public Material material;
    public ObiRopeSection section;
    private ObiRope RightRope;
    private ObiRopeBlueprint blueprint;
    private ObiRopeExtrudedRenderer ropeRenderer;

    private ObiRopeCursor cursor;

    private RaycastHit hookAttachment;

    private bool firstPush = false;

    void Awake()
    {

        // Create both the rope and the solver:	
        // ロープとソルバーの両方を作成します。	
        RightRope = gameObject.AddComponent<ObiRope>();
        ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = section;
        ropeRenderer.uvScale = new Vector2(1, 5);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        RightRope.GetComponent<MeshRenderer>().material = material;
        //rope.distanceConstraintsEnabled = false;

        // Setup a blueprint for the rope:
        // ロープのブループリントを設定する。	
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;

        // Tweak rope parameters:
        // ロープのパラメータを調整する。	
        RightRope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        // ロープの長さを変えることができるカーソルを追加。
        cursor = RightRope.gameObject.AddComponent<ObiRopeCursor>();
        cursor.cursorMu = 0;
        cursor.direction = true;
    }

    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }

    /**
	 * Raycast against the scene to see if we can attach the hook to something.
	 * 
	 * シーンに対してレイキャストを行い、フックを何かに取り付けられるかどうかを確認します。
	 */
    private void LaunchLeftHook()
    {

        // Get the mouse position in the scene, in the same XY plane as this object:
        // このオブジェクトと同じXY平面上の、シーン内のマウスの位置を取得する。
        Vector3 mouse = Input.mousePosition;
        mouse.z = (transform.position.z - Camera.main.transform.position.z) * 90;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        // Get a ray from the character to the mouse:
        // キャラクターからマウスへのレイを取得します。
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast to see what we hit:
        // レイキャストでヒット商品を確認
        if (Physics.Raycast(ray, out hookAttachment))
        {

            // We actually hit something, so attach the hook!
            // 実際に何かにぶつかったので、フックを付けてみてください。
            StartCoroutine(AttachLeftHook());
        }

    }

    private IEnumerator AttachLeftHook()
    {
        yield return 0;
        Vector3 localHit = RightRope.transform.InverseTransformPoint(hookAttachment.point);

        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);

        // Procedurally generate the rope path (a simple straight line):
        // ロープの経路（単純な直線）をプロシージャルに生成します。
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        // Generate the particle representation of the rope (wait until it has finished):
        // ロープのパーティクル表現を生成します（完了するまで待ちます）。
        yield return blueprint.Generate();

        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        // ブループリントを設定します（これにより、ソルバーにパーティクルや制約条件が追加され、シミュレーションが開始されます）。
        RightRope.ropeBlueprint = blueprint;
        RightRope.GetComponent<MeshRenderer>().enabled = true;

        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        // ロープの両端をピンで固定する（これでキャラクターとロープの双方向のやりとりが可能になる）。
        var pinConstraints = RightRope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(RightRope.solverIndices[0], RightHand, transform.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(RightRope.solverIndices[blueprint.activeParticleCount - 1], hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);

        RightRope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }

    private void DetachHook()
    {
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        // ロープのブループリントをnullに設定します（前のブループリントがあれば、ソルバーから自動的に削除されます）。
        RightRope.ropeBlueprint = null;
        RightRope.GetComponent<MeshRenderer>().enabled = false;
    }


    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(1))
        {
            if (!RightRope.isLoaded)
            {
                LaunchLeftHook();
                firstPush = true;
            }
        }

        if (firstPush == true)
            if (Input.GetMouseButtonDown(1))
            {
                DetachHook();
            }

        if (RightRope.restLength <= 5)
        {
            hookExtendRetractSpeed = 0;
        }
        else
        {
            hookExtendRetractSpeed = 1;
        }
    }

    private void FixedUpdate()
    {
        if (RightRope.isLoaded)
        {
            cursor.ChangeLength(RightRope.restLength - hookExtendRetractSpeed);
        }
    }
}
