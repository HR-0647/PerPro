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
 * Obi Rope���g����2.5D�Q�[���p�̃O���b�v�����O�t�b�N���쐬������@�������T���v���R���|�[�l���g�ł��B
   �R�[�h��95���̓O���b�v�����O�t�b�N�̃��W�b�N�i���[�U�[�̓��́A�V�[���̃��C�L���X�e�B���O�A���ˁA�t�b�N�̎��t���Ȃǁj�ƃp�����[�^�̐ݒ�ł��B
   �����^�C���Ŋ��S�ɑт��g�p������@���������߂ł��B����͎��ۂ̃V�i���I�ł͎��p�I�ł͂Ȃ���������܂���B
   �������A���̕��@��������Ă��܂��B

   �Ȃ��A�O���b�v���_�C�i�~�N�X�Ɏ��ۂ̃��[�v�V�~�����[�V�������g�����ǂ����͋c�_�̗]�n������܂��B�ʏ��
   �P���ȃo�l�̕������\�I�ɂ����䐫�̖ʂł��D��Ă��܂��B

   �V�[���Ƃ̕��G�ȃC���^���N�V�������K�v�ȏꍇ�́A�����ɃW�I���g���x�[�X�̃A�v���[�`�iWorms ninja rope�Ȃǁj���K���Ă��܂��B
   (Worms ninja rope�̂悤��)�����ȃW�I���g���x�[�X�̃A�v���[�`��
   �󋵂ɂ���Ă͐������I���ƂȂ�ł��傤�B
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
        // ���[�v�ƃ\���o�[�̗������쐬���܂��B	
        RightRope = gameObject.AddComponent<ObiRope>();
        ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = section;
        ropeRenderer.uvScale = new Vector2(1, 5);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        RightRope.GetComponent<MeshRenderer>().material = material;
        //rope.distanceConstraintsEnabled = false;

        // Setup a blueprint for the rope:
        // ���[�v�̃u���[�v�����g��ݒ肷��B	
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;

        // Tweak rope parameters:
        // ���[�v�̃p�����[�^�𒲐�����B	
        RightRope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        // ���[�v�̒�����ς��邱�Ƃ��ł���J�[�\����ǉ��B
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
	 * �V�[���ɑ΂��ă��C�L���X�g���s���A�t�b�N�������Ɏ��t�����邩�ǂ������m�F���܂��B
	 */
    private void LaunchLeftHook()
    {

        // Get the mouse position in the scene, in the same XY plane as this object:
        // ���̃I�u�W�F�N�g�Ɠ���XY���ʏ�́A�V�[�����̃}�E�X�̈ʒu���擾����B
        Vector3 mouse = Input.mousePosition;
        mouse.z = (transform.position.z - Camera.main.transform.position.z) * 90;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        // Get a ray from the character to the mouse:
        // �L�����N�^�[����}�E�X�ւ̃��C���擾���܂��B
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Raycast to see what we hit:
        // ���C�L���X�g�Ńq�b�g���i���m�F
        if (Physics.Raycast(ray, out hookAttachment))
        {

            // We actually hit something, so attach the hook!
            // ���ۂɉ����ɂԂ������̂ŁA�t�b�N��t���Ă݂Ă��������B
            StartCoroutine(AttachLeftHook());
        }

    }

    private IEnumerator AttachLeftHook()
    {
        yield return 0;
        Vector3 localHit = RightRope.transform.InverseTransformPoint(hookAttachment.point);

        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);

        // Procedurally generate the rope path (a simple straight line):
        // ���[�v�̌o�H�i�P���Ȓ����j���v���V�[�W�����ɐ������܂��B
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit, -localHit.normalized, localHit.normalized, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        // Generate the particle representation of the rope (wait until it has finished):
        // ���[�v�̃p�[�e�B�N���\���𐶐����܂��i��������܂ő҂��܂��j�B
        yield return blueprint.Generate();

        // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
        // �u���[�v�����g��ݒ肵�܂��i����ɂ��A�\���o�[�Ƀp�[�e�B�N���␧��������ǉ�����A�V�~�����[�V�������J�n����܂��j�B
        RightRope.ropeBlueprint = blueprint;
        RightRope.GetComponent<MeshRenderer>().enabled = true;

        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        // ���[�v�̗��[���s���ŌŒ肷��i����ŃL�����N�^�[�ƃ��[�v�̑o�����̂��Ƃ肪�\�ɂȂ�j�B
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
        // ���[�v�̃u���[�v�����g��null�ɐݒ肵�܂��i�O�̃u���[�v�����g������΁A�\���o�[���玩���I�ɍ폜����܂��j�B
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
