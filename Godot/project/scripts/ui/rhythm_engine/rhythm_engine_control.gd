extends Control

@export_group("Metronome")
@export var metronome_sounds: Array[AudioStream]
@export var metronome_end_command: AudioStream

@export_group("Drums")
@export var drum_perfect_sounds: Array[AudioStream]
@export var drum_normal_sounds: Array[AudioStream]

@onready var metronome_player = $MetronomePlayer
@onready var drum_player = $DrumPlayer

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

var last_beat = -1

func play_metronome(beat, left_command_beat):
	if last_beat != beat:
		last_beat = beat
		print("salut")
		
		if left_command_beat == 1:
			metronome_player.stream = metronome_end_command
			metronome_player.pitch_scale = 1.0
		else:
			metronome_player.stream = metronome_sounds[beat % metronome_sounds.size()]
			metronome_player.pitch_scale = 1.025 if (beat % 2 == 0) else 0.975
			
		metronome_player.volume_db = 0 if (left_command_beat == 1 or left_command_beat <= 0) else -6.5
			
		metronome_player.stop()
		metronome_player.play()
		
func play_drum(key, score):
	var sounds = drum_perfect_sounds
	if score > 0.16:
		sounds = drum_normal_sounds
	
	drum_player.stream = sounds[key]
	drum_player.stop()
	drum_player.play()
