@tool

extends Control

@export_range(0., 1.) var progress: float = 0.

@onready var gauges = [
	$"0_TopRight",
	$"1_BottomRight",
	$"2_BottomLeft",
	$"3_TopLeft"
]

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var i = 0
	var next_active = false
	for gauge in gauges:
		gauge.sub_progress = clamp((progress * gauges.size()) - i, 0., 1.)
		gauge.active = next_active
		
		next_active = gauge.sub_progress >= 1.0
		
		i += 1
