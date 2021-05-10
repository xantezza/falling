using Godot;
using System;

public class Main : Spatial
{
	public float time = 0;
	public float trackLenght = 100;
	float moveSpeed = 0.02f;
	float high;
	float mid;
	float low;	

	float health = 100;
	float bottomDb = -30;
	ProgressBar healthBar;
	ProgressBar progressBar;
	Area head;
	PackedScene obstacleScene;
	KinematicBody obstacle;
	Timer timer;
	SpatialMaterial material;
	AudioEffectSpectrumAnalyzerInstance spectrum;
	Vector2 moveDirection = new Vector2(0, 0);

	public override void _Ready()
	{
		head = GetNode<Area>("Head");
		healthBar = GetNode<ProgressBar>("Control/Health");
		progressBar = GetNode<ProgressBar>("Control/Progress");
		material = GD.Load<SpatialMaterial>("res://Materials/1.tres");
		obstacleScene = GD.Load<PackedScene>("res://Scenes/ObstacleBody.tscn");
		timer = GetNode<Timer>("Spawner");	
	}

	public override void _PhysicsProcess(float delta)
	{
		moveDirection = new Vector2(
			-Convert.ToInt32(Input.IsActionPressed("ui_right")) + Convert.ToInt32(Input.IsActionPressed("ui_left")),
			-Convert.ToInt32(Input.IsActionPressed("ui_down")) + Convert.ToInt32(Input.IsActionPressed("ui_up"))
			);
		if (moveDirection != new Vector2(0, 0))
		{
			moveDirection = moveDirection.Normalized();

			moveDirection = new Vector2(
										moveDirection.x,
										moveDirection.y
										) * moveSpeed;		

			var a = new Vector2(head.Translation.x, head.Translation.y) + moveDirection;
			if (a.DistanceTo(new Vector2(0, 0)) < 0.9)
			{
				head.Translation += new Vector3( moveDirection.x,
												 moveDirection.y,
												 0)*delta*100;
			}
			else
			{
				health -= 0.5f;
				
				if (health < 0) GetTree().ReloadCurrentScene();
			}
		}

		
		material.AlbedoColor = MusicToRGB();
		time += delta;
		
		healthBar.Value = Mathf.MoveToward((float)healthBar.Value, health, 0.5f);
		if (Input.IsActionJustPressed("ui_cancel"))
			GetTree().Quit();
	}
	public Color MusicToRGB(){

		spectrum = (AudioEffectSpectrumAnalyzerInstance)AudioServer.GetBusEffectInstance(0, 0);
		high = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(5000, 20000).Length()), bottomDb, 10, 0, 255);
		mid = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(500, 5000).Length()), bottomDb, 10, 0, 255);
		low = RangeLerp(GD.Linear2Db(spectrum.GetMagnitudeForFrequencyRange(10, 200).Length()), bottomDb, 0, 0, 255);

		return Color.Color8
		(
			(byte)high,
			(byte)mid,
			(byte)low,
			255
		);
	}
	public static float RangeLerp(float value, float istart, float istop, float ostart, float ostop)
	{
		return ostart + (ostop - ostart) * value / (istop - istart);
	}
	void _on_Spawner_timeout()
	{
		Spatial spatial = GetNode<Spatial>("Spatial/Tunnel/Spatial");
		obstacle = (KinematicBody)obstacleScene.Instance();
		AddChild(obstacle);
		obstacle.Translation = new Vector3(0, 0, 52);

		var timer = GetNode<Timer>("Spawner");
		timer.WaitTime = RangeLerp(time, 1, trackLenght, 0.5f, 0.2f);
		Console.WriteLine(timer.WaitTime);
		if (progressBar.Value > progressBar.MaxValue) GetTree().ReloadCurrentScene();
		progressBar.Value += timer.WaitTime;
	}

	void _on_ObstacleKiller_body_entered(Node body)
	{
		GetNode<Label>("Control/Label2").Text += "1";
		body.QueueFree();
	}
	void _on_Head_body_entered(Node body)
	{
		health -= 20;
		if (health < 0) GetTree().ReloadCurrentScene();
	}
	void _on_ItemList_item_selected(int index){

		ItemList itemList = GetNode<ItemList>("Control/ScrollContainer/ItemList");
		itemList.Hide();

		AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer3D");
		
		audioPlayer.Stream = GD.Load<AudioStream>("res://Audio/" + itemList.GetItemText(index) + ".ogg");
		audioPlayer.Play();
		trackLenght = audioPlayer.Stream.GetLength();
		progressBar.MaxValue = trackLenght;

		GetNode<Label>("Control/Label").Hide();
		GetNode<Timer>("Spawner").Start();
	}
	void _on_VSlider_value_changed(float value){
		bottomDb = value;
	}
}
