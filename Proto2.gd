extends KinematicBody



export var acceleration = 15
export var speed = 16
export var velocity = Vector3()

export var gravity = -60
export var maxGravity = -150

export(float, 0.1, 1) var mouseSensitivity : float = 0.3
export(float, -90, 0) var minPitch : float = -90
export(float, 0, 90) var maxPitch : float = 90

onready var anim = $AnimationPlayer
onready var animTree = $AnimationTree
onready var camera_pivot = $CameraPivot
onready var camera = $CameraPivot/CameraBoom/Camera
onready var skeleton = get_node("Armature/Skeleton")

var headbone
var initialHeadTransform
var attacking

# Called when the node enters the scene tree for the first time.
func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	OS.set_window_maximized(true)
	
	headbone = skeleton.find_bone("head")
	camera.translation = skeleton.get_bone_global_pose(headbone).origin
	initialHeadTransform = skeleton.get_bone_pose(headbone)


func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		get_tree().quit()
		
	var targetDir = Vector2(0, 0)
	if Input.is_action_pressed("move_forward"):
		targetDir.y += 1
	if Input.is_action_pressed("move_backward"):
		targetDir.y -= 1
	if Input.is_action_pressed("move_left"):
		targetDir.x += 1
	if Input.is_action_pressed("move_right"):
		targetDir.x -= 1
	if Input.is_action_pressed("Attack"):
		attacking = true

	
	set_anim(targetDir)
	
	targetDir = targetDir.normalized().rotated(-rotation.y)
	velocity.x = lerp(velocity.x, targetDir.x * speed, acceleration * delta)
	velocity.z = lerp(velocity.z, targetDir.y * speed, acceleration * delta)
	
	velocity.y += gravity * delta
	if velocity.y < maxGravity:
		velocity.y = maxGravity
	
	move_and_slide(velocity,Vector3(0, 1, 0))
	
	if is_on_floor() and velocity.y < 0:
		velocity.y = 0
	
	var currentHeadTransform = initialHeadTransform.rotated(Vector3(-1,0,0), camera_pivot.rotation.x)
	skeleton.set_bone_pose(headbone, currentHeadTransform)

func _input(event):
	if event is InputEventMouseMotion:
		var movement = event.relative
		camera_pivot.rotation.x += -deg2rad(movement.y * mouseSensitivity)
		camera_pivot.rotation.x = clamp(camera_pivot.rotation.x, deg2rad(minPitch), deg2rad(maxPitch))
		rotation.y += -deg2rad(movement.x * mouseSensitivity)

func set_anim(dir):
	animTree.set("parameters/BlendSpace2D/blend_position", dir)
	if attacking == true:
		animTree.set("parameters/OneShot/active", attacking)
#	if dir == Vector2(0, 0) and anim.current_animation != "idle":
#		anim.play("Idle", 0.1)
#	elif dir == Vector2(0, 1) and not (anim.current_animation == "forward" and anim.get_playing_speed() > 0):
#		anim.play("forward", 0.1)
#	elif dir == Vector2(1, 1) and not (anim.current_animation == "forwardleft" and anim.get_playing_speed() > 0):
#		anim.play("forwardleft", 0.1)
#	elif dir == Vector2(-1, 1) and not (anim.current_animation == "forwardright" and anim.get_playing_speed() > 0):
#		anim.play("forwardright", 0.1)
#	elif dir == Vector2(1, 0) and anim.current_animation != "left":
#		anim.play("left", 0.1)
#	elif dir == Vector2(-1, 0) and anim.current_animation != "right":
#		anim.play("right", 0.1)
#	elif dir == Vector2(0, -1) and not (anim.current_animation == "forward" and anim.get_playing_speed() < 0):
#		anim.play_backwards("forward", 0.1)
#	elif dir == Vector2(1, -1) and not (anim.current_animation == "forwardright" and anim.get_playing_speed() < 0):
#		anim.play_backwards("forwardright", 0.1)
#	elif dir == Vector2(-1, -1) and not (anim.current_animation == "forwardleft" and anim.get_playing_speed() < 0):
#		anim.play_backwards("forwardleft", 0.1)
		
