extends Control

@onready var pressures: Array[Node] = [
	$up,
	$down,
	$right,
	$left
]

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func play(index):
	pressures[index].get_node("Control/AnimationPlayer").stop()
	pressures[index].get_node("Control/AnimationPlayer").play("trigger")

func _process(delta):
	var index = -1
	
	if Input.is_key_pressed(KEY_KP_8):
		play(0)
	if Input.is_key_pressed(KEY_KP_2):
		play(1)
	if Input.is_key_pressed(KEY_KP_6):
		play(2)
	if Input.is_key_pressed(KEY_KP_4):
		play(3)
