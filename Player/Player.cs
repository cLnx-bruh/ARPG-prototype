using Godot;
using System;

public class Player : KinematicBody
{
    [Export]
    private float speed = 20f;
    [Export]
    private float groundAcceleration = 15f;
    [Export]
    private float airAcceleration = 5f;
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

    private Spatial mesh;

    private AnimationTree animationTree;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.cameraPivot = FindNode("CameraPivot", true, true) as Spatial;
        this.camera = FindNode("Camera", true, true) as Camera;
        this.mesh = FindNode("MeshInstance", true, true) as Spatial;
        this.animationTree = FindNode("AnimationTree", true, true) as AnimationTree;

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.SetMouseMode(Input.MouseMode.Visible);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        this.HandleMovement(delta);
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

    private void HandleMovement(float delta)
    {
        float horizontal = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        float forward = Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward");

        Vector3 inputDirection = new Vector3(horizontal, 0, forward);

        this.mesh.RotationDegrees = Vector3.Zero;
        if (inputDirection.z != 0)
        {
            int forwardDiagonalDirection = Math.Sign(inputDirection.x) * -Math.Sign(inputDirection.z) * 1;

            float newYDegrees = this.mesh.RotationDegrees.z - 45f * -forwardDiagonalDirection;
            if (Mathf.Abs(0 - newYDegrees) <= 45f)
            {
                this.mesh.RotationDegrees = new Vector3(0, -newYDegrees, 0);
            }
        }
        Vector3 localDirection = inputDirection.Normalized().Rotated(Vector3.Up, this.Rotation.y);

        Vector3 velocity = Vector3.Zero;
        if (localDirection.Length() > 0.1f)
        {
            float acceleration = this.IsOnFloor()? this.groundAcceleration : this.airAcceleration;
            velocity = this.velocity.LinearInterpolate(localDirection * speed, acceleration * delta);
        }

        if (this.IsOnFloor()) {
            this.yVelicity = -0.01f; 
        } else {
            this.yVelicity = Mathf.Clamp(this.yVelicity - this.gravity, -this.maxTerminalVelocity, this.maxTerminalVelocity);
        }

        if (Input.IsActionJustPressed("jump") && this.IsOnFloor()) {
            yVelicity = jumpPower;
        }

        velocity.y = yVelicity;
        this.velocity = this.MoveAndSlide(velocity, Vector3.Up);

        this.animationTree.Set("parameters/BlendSpace2D/blend_position", new Vector2(inputDirection.x, inputDirection.z));
    }
}
