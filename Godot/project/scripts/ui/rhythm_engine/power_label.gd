extends Label

@export var is_using_power: bool = false
@export_range(0., 1.) var power_progress: float = 0.
@export var power_level: int = 0
@export var is_at_max_level: bool = false

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


var flames_power: float = 0.

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var target_flames_power: float = (1.0 
			if (is_at_max_level and power_progress > 0.999) 
			else (0.25 if is_using_power else 0.0))
			
	flames_power = lerp(flames_power, target_flames_power, delta * 8.)
	
	var shader = material as ShaderMaterial
	shader.set_shader_parameter("progress", power_progress)
	shader.set_shader_parameter(
		"flames_power",
		flames_power
	)
	
	text = str(power_level)
	pass
