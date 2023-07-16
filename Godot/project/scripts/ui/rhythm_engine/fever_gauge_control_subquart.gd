@tool

extends Panel

@export_range(0., 1.) var sub_progress: float
@export var active: bool

@onready var gauge = $Gauge

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):	
	var shader_m = gauge.material as ShaderMaterial
	shader_m.set_shader_parameter("gauge_progress", sub_progress)
	if sub_progress > 0.999:
		shader_m.set_shader_parameter("edge_power", 0.)
	else:
		shader_m.set_shader_parameter("edge_power", 1.) 
		
	self_modulate.a = lerp(self_modulate.a, 1.0 if (active or sub_progress > 0.0) else 0.0, delta * 10.)
	gauge.self_modulate.v = lerp(gauge.self_modulate.v, 1.0 if (sub_progress >= 1.0) else 0.8, delta * 10.)
