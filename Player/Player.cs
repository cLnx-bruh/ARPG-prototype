using Godot;
using System;

public class Player : KinematicBody
{
    private readonly float DIAGONAL_TURN_ANGLE = 45f;

    [Export]
    private float speed = 20f;
    [Export]
    private float groundAcceleration = 15f;
    [Export]
    private float airAcceleration = 5f;
    [Export]
    private float diagonalTurnRate = 5f;
    [Export]
    private float gravity = 0.98f;
    [Export]
    private float maxTerminalVelocity = 54f;
    [Export]
    private float jumpPower = 20f;

    [Export]
    private float mouseSensitivy = 0.3f;
    [Export]
    private float minPitch = -90f;
    [Export]
    private float maxPitch = 90f;

    private Vector3 velocity;
    private float yVelicity;

    private Spatial cameraPivot;
    private Camera camera;

    private Spatial armature;

    private AnimationTree animationTree;

    private AnimationPlayer animationPlayer;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.cameraPivot = FindNode("CameraPivot", true, true) as Spatial;
        this.camera = FindNode("Camera", true, true) as Camera;
        this.armature = FindNode("Armature", true, true) as Spatial;
        this.animationTree = FindNode("AnimationTree", true, true) as AnimationTree;
        this.animationPlayer = FindNode("AnimationPlayer", true, true) as AnimationPlayer;

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    bool showCursor = true;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (this.showCursor)
            {
                Input.SetMouseMode(Input.MouseMode.Visible);
            }
            else
            {
                Input.SetMouseMode(Input.MouseMode.Captured);
            }

            this.showCursor = !this.showCursor;
        }

        if (Input.IsMouseButtonPressed(1) && !this.IsAttacking()) {
            this.StartAttacking();
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector3 inputDirection = this.GetInputDirection();
        this.HandleDiagonalRotation(inputDirection, delta);
        this.HandleMovement(inputDirection, delta);
        this.HandleAnimations(inputDirection, delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            this.HandleRotation(@event as InputEventMouseMotion);
        }
    }

    private void HandleRotation(InputEventMouseMotion mouseMotion)
    {
        this.RotationDegrees -= new Vector3(0, mouseMotion.Relative.x * this.mouseSensitivy, 0);
        float cameraPivotXRotationDegree = this.cameraPivot.RotationDegrees.x - mouseMotion.Relative.y * this.mouseSensitivy;
        cameraPivotXRotationDegree = Mathf.Clamp(cameraPivotXRotationDegree, this.minPitch, this.maxPitch);
        this.cameraPivot.RotationDegrees = new Vector3(cameraPivotXRotationDegree, 0, 0);
    }

    private void HandleMovement(Vector3 inputDirection, float delta)
    {
        Vector3 velocity = Vector3.Zero;

        Vector3 localDirection = inputDirection.Normalized().Rotated(Vector3.Up, this.Rotation.y);

        if (localDirection.Length() > 0.1f)
        {
            float acceleration = this.IsOnFloor() ? this.groundAcceleration : this.airAcceleration;
            velocity = this.velocity.LinearInterpolate(localDirection * speed, acceleration * delta);
        }

        if (this.IsOnFloor())
        {
            this.yVelicity = -0.01f;
        }
        else
        {
            this.yVelicity = Mathf.Clamp(this.yVelicity - this.gravity, -this.maxTerminalVelocity, this.maxTerminalVelocity);
        }

        if (Input.IsActionJustPressed("jump") && this.IsOnFloor())
        {
            yVelicity = jumpPower;
        }

        velocity.y = yVelicity;
        this.velocity = this.MoveAndSlide(velocity, Vector3.Up);
    }

    private void HandleDiagonalRotation(Vector3 inputDirection, float delta)
    {
        int diagonalDirection = Math.Sign(inputDirection.x) * Math.Sign(inputDirection.z);
        if (diagonalDirection != 0)
        {
            Vector3 desiredRotation = new Vector3(0, diagonalDirection * DIAGONAL_TURN_ANGLE, 0);
            Vector3 newROtation = this.armature.RotationDegrees.LinearInterpolate(desiredRotation, this.diagonalTurnRate * delta);
            this.armature.RotationDegrees = newROtation;
        }
        else if (this.armature.RotationDegrees.y != 0)
        {
            Vector3 newRotation = this.armature.RotationDegrees.LinearInterpolate(Vector3.Zero, this.diagonalTurnRate * delta);
            this.armature.RotationDegrees = newRotation;
        }
    }

    private void HandleAnimations(Vector3 inputDirection, float delta)
    {
        Vector2 currentBlend = (Vector2)this.animationTree.Get("parameters/BlendSpace2D/blend_position");
        Vector2 blendDirection = new Vector2(inputDirection.x, inputDirection.z);
        Vector2 newBlend = currentBlend.LinearInterpolate(blendDirection, 10 * delta);
        this.animationTree.Set("parameters/BlendSpace2D/blend_position", newBlend);
    }

    private Vector3 GetInputDirection()
    {
        float horizontal = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        float forward = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");

        return new Vector3(horizontal, 0, forward);
    }

    private bool IsAttacking() {
        return  (bool) this.animationTree.Get("parameters/OneShot/active");
    }

    private void StartAttacking() {
        this.animationTree.Set("parameters/OneShot/active", true);
    }

    public void StopAttacking() {
        this.animationTree.Set("parameters/OneShot/active", false);
    }
}
