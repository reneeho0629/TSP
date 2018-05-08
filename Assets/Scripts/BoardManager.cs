﻿//coordinates are fixed, but does that mean we're going to have problems with distances?
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour {




	//Resoultion width and Height
	//CAUTION! Modifying this does not modify the Screen resolution. This is related to the unit grid on Unity.
	public static int resolutionWidth = 1024;
	public static int resolutionHeight = 768;

	//Number of Columns and rows of the grid (the possible positions of the Items) note: these are default values.
	public static int columns = 4;
	public static int rows = 4;

	//The Item radius. This is used to avoid superposition of Items.
	//public static float KSItemRadius = 1.5f;

	//Timer width
	//public static float timerWidth =400;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	//The method to be used to place Items randomly on the grid.
	//1. Choose random positions from full grid. It might happen that no placement is found and the trial will be skipped.
	//2. Choose randomly out of 10 positions. A placement is guaranteed

	//Prefab of the Item interface configuration
	public static GameObject TSPItemPrefab;

	//Prefab of the Item interface configuration
	public static GameObject LineItemPrefab;

	//The possible positions of the Items;
	private List <Vector3> gridPositions = new List<Vector3> ();

	//Counter
	public Text DistanceText;

	//Coordinate vectors for this trial. CURRENTLY SET UP TO ALLOW ONLY INTEGERS.
	private float[] cox;
	private float[] coy;
	private int[] cities;
	private int[,] distances;

	//The answer Input by the player
	//0:No / 1:Yes / 2:None
	public static int answer;

	private String question;

	//Should the key be working?
	public static bool keysON = false;

	//Reset button
	public Button Reset;

	//necessary?
	//If randomization of buttons:
	//1: No/Yes 0: Yes/No
	public static int randomYes;//=Random.Range(0,2);

	public static int distanceTravelledValue;

	//Structure with the relevant parameters of an Item.
	//gameItem: is the game object
	//coorValue1: The coordinates of one of the corners of the encompassing rectangle of the Value Part of the Item. The coordinates are taken relative to the center of the Item.
	//coorValue2: The coordinates of the diagonally opposite corner of the Value Part of the Item.
	//coordWeight1 and coordWeight2: Same as before but for the weight part of the Item.
	//botncitoW: button attached to the weight
	//botncitoV: button attached to the Value (Bill)
	//ItemNumber: a number between 1 and the number of Items. It corresponds to the index in the weight's (and value's) vector.
	private struct Item
	{
		public GameObject gameItem;
		public Vector2 center;
		public int CityNumber;
		public Button CityButton;
	}
//	public Button LineButton;

	//The Items for the scene are stored here.
	private Item[] Items;


	//Function which takes coordinates from input files and converts it to coordinates on the unity grid
	//unity origin x=-476, y=-344, which means opposite corner is x=476 and y=344
	//minimum input coordinate is (0,0), maximum input coordinate is (952,688)
	private List<Vector2> coordinateconvertor(float[] cox, float[] coy)
	{
		List<Vector2> unitycoordinates = new List<Vector2> ();
		for (int i = 0; i < cox.Length; i++) {
//			unitycoordinates.Add (new Vector2 ((float)((cox [i] / cox.Max ()) * 952 - 476) / 100, (float)((coy [i] / coy.Max ()) * 688 - 344) / 100));
//			Debug.Log ((float)(cox [i] / cox.Max ()) * 952 - 476);

			//x=0 in coord leads to x=-384 in unity, x=1024 leads to x=599.04 - so a 1 unit increase in coord leads to a 0.96 unit increase in unity
			//y=0 in coord leads to y=-288 in unity, y=768 leads to y=449.28 - so a 1 unit increase in coord leads to a 0.96 unit increase in unity
			//x-1 below leads to x=-384 in unity, x=1024 leads to x=599.04 - so a 1 unit increase in coord leads to a 0.96 unit increase in unity
			//y-1 below leads to 
			unitycoordinates.Add (new Vector2 ((float)((cox[i] / cox.Max()) * 1024-110f)/100,(float)((coy[i] / coy.Max()) * 710-80f)/100));

//			unitycoordinates.Add (new Vector2 ((float)cox[i], (float)coy[i]));
//			Debug.Log (((float)(cox [i] / cox.Max ()) * 1024 - 512) / 100);
		}
			return unitycoordinates;

	}


	//This Initializes the GridPositions which are the possible places where the Items will be placed.
	void InitialiseList ()
	{
		gridPositions.Clear ();
		//Simple 9 positions grid. 
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {	
				float xUnit = (float)(resolutionWidth / 100) / columns;
				float yUnit = (float)(resolutionHeight / 100) / rows;
				//1 x unit = 320x positions in unity, whilst 1 y unit = 336y grid positions in unity
				//gridPositions.Add (new Vector3 ((x-0.8f) * xUnit, (y-0.7619f) * yUnit, 0f)); //- top left value in the right spot, everything else not quite
				gridPositions.Add (new Vector3 ((x) * xUnit, (y+0.4f) * yUnit, 0f));
				Debug.Log ("x" + x + " y" + y);
			}
		}
	}
		
	public List<Vector2> unitycoord = new List<Vector2> ();

	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded
	void setTSPInstance()
	{
		int randInstance = GameManager.instanceRandomization[GameManager.TotalTrial-1];

		//		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		//		String question = "Can you obtain at least $" + GameManager.satinstances[randInstance].profit + " with at most " + GameManager.satinstances[randInstance].capacity +"kg?";
		//		Quest.text = question;

		//necessary?
		//question = "Can you pack $" + GameManager.satinstances[randInstance].profit + " if your capacity is " + GameManager.satinstances[randInstance].capacity +"kg?";
		question = "Max: " + GameManager.tspinstances[randInstance].maxdistance +"km";
		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		Quest.text = question;
		DistanceText = GameObject.Find ("DistanceText").GetComponent<Text>();
		Reset = GameObject.Find("Reset").GetComponent<Button>();
		Reset.onClick.AddListener(ResetClicked);

		//question = " Max: " + System.Environment.NewLine + GameManager.satinstances[randInstance].capacity +"kg ";

		cox = GameManager.tspinstances [randInstance].coordinatesx;
		coy = GameManager.tspinstances [randInstance].coordinatesy;
		unitycoord = coordinateconvertor(cox,coy);

		cities = GameManager.tspinstances [randInstance].cities;
		distances = GameManager.tspinstances [randInstance].distancematrix;

		TSPItemPrefab = (GameObject)Resources.Load ("TSPItem");
		LineItemPrefab = (GameObject)Resources.Load ("LineButton");

		int objectCount =coy.Length;
		Items = new Item[objectCount];
		for(int i=0; i < objectCount;i=i+1)
		{
			//int objectPositioned = 0;
			Item ItemToLocate = generateItem (i, unitycoord[i]);//66: Change to different Layer?
			Items[i] = ItemToLocate;
		}
	}

	/// <summary>
	/// Instantiates an Item and places it on the position from the input
	/// </summary>
	/// <returns>The Item structure</returns>
	/// The Item placing here is temporary; The real placing is done by the placeItem() method.
	Item generateItem(int ItemNumber ,Vector2 randomPosition)
	{
		//Instantiates the Item and places it.
		GameObject instance = Instantiate (TSPItemPrefab, randomPosition, Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		Item ItemInstance = new Item();
		ItemInstance.gameItem = instance;//.gameObject;
		ItemInstance.CityButton = ItemInstance.gameItem.GetComponent<Button> ();
		ItemInstance.CityNumber = cities[ItemNumber];
		ItemInstance.center = randomPosition;

		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		placeItem (ItemInstance,randomPosition);

		//Goes from 1 to numberOfItems
		//note: not sure what this is being used for, so check that's it's ok before using it elsewhere

		return(ItemInstance);

	}


	/// <summary>
	/// Places the Item on the input position
	/// </summary>
	void placeItem(Item ItemToLocate, Vector2 position){
		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		ItemToLocate.gameItem.transform.position = position;
		ItemToLocate.CityButton.onClick.AddListener(delegate{ClickOnCity(ItemToLocate);});
	}	



	//Returns a random position from the grid and removes the Item from the list.
	Vector3 RandomPosition()
	{
		int randomIndex=Random.Range(0,gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);
		return randomPosition;
	}


	/// Macro function that initializes the Board
	public void SetupScene(string sceneToSetup)
	{
		if (sceneToSetup == "Trial") 
		{
			//InitialiseList();
			previouscities.Clear();
			itemClicks.Clear ();
			GameManager.Distancetravelled = 0;
			distanceTravelledValue = 0;
			setTSPInstance ();

			keysON = true;
		} else if(sceneToSetup == "TrialAnswer")
		{
			answer = 2;
			//setQuestion ();
			RandomizeButtons ();
			keysON = true;

			//seeGrid();
		}

	}

	//Updates the timer rectangle size accoriding to the remaining time.
	public void updateTimer()
	{
		// timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
		// timer.sizeDelta = new Vector2 (timerWidth * (GameManager.tiempo / GameManager.totalTime), timer.rect.height);
		if (GameManager.escena != "SetUp" || GameManager.escena == "InterTrialRest" || GameManager.escena == "End") 
		{
			Image timer = GameObject.Find ("Timer").GetComponent<Image> ();
			timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
		}

	}

	//Sets the triggers for pressing the corresponding keys
	//123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	//changeToNextScene(answer) on game manager
	//necessary: this was imported from decision version
	private void setKeyInput(){

		if (GameManager.escena == "Trial") {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				GameManager.saveTimeStamp ("ParticipantSkip");
				GameManager.changeToNextScene (itemClicks,0,0);
			}
		} else if (GameManager.escena == "TrialAnswer") 
		{
			//1: No/Yes 0: Yes/No
			if (randomYes == 1) {
				if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					//Left
					//GameManager.changeToNextScene (0, randomYes);
					keysON = false;
					answer=0;
					GameObject boto = GameObject.Find("LEFTbutton") as GameObject;
					highlightButton(boto);
					SceneManagerFunctions.setTimeStamp ();
					GameManager.changeToNextScene (itemClicks,0,1);
				} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					//Right
					//GameManager.changeToNextScene (1, randomYes);
					keysON = false;
					answer=1;
					GameObject boto = GameObject.Find("RIGHTbutton") as GameObject;
					highlightButton(boto);
					SceneManagerFunctions.setTimeStamp ();
					GameManager.changeToNextScene (itemClicks,1,1);
				}
			} else if (randomYes == 0) {
				if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					//Left
					//GameManager.changeToNextScene (1, randomYes);
					keysON = false;
					answer=1;
					GameObject boto = GameObject.Find("LEFTbutton") as GameObject;
					highlightButton(boto);
					SceneManagerFunctions.setTimeStamp ();
					GameManager.changeToNextScene (itemClicks,1,0);
				} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					//Right
					//GameManager.changeToNextScene (0, randomYes);
					keysON = false;
					answer = 0;
					GameObject boto = GameObject.Find("RIGHTbutton") as GameObject;
					highlightButton(boto);
					SceneManagerFunctions.setTimeStamp ();
					GameManager.changeToNextScene (itemClicks,0,0);
				}
			}
		} else if (GameManager.escena == "SetUp") {
			if (Input.GetKeyDown (KeyCode.Space)) {
				SceneManagerFunctions.setTimeStamp ();
				GameManager.changeToNextScene (itemClicks,0,0);
			}
		}
	}

	private void highlightButton(GameObject butt)
	{
		Text texto = butt.GetComponentInChildren<Text> ();
		texto.color = Color.gray;
	}
		

	// Use this for initialization
	void Start () 
	{

	}

	// Update is called once per frame
	void Update () 
	{
		if (keysON) 
		{
			setKeyInput ();
		}

	}




	//necessary?
	//Randomizes YES/NO button positions (left or right) and allocates corresponding script to save the correspondent answer.
	//1: No/Yes 0: Yes/No
	void RandomizeButtons()
	{
		Button btnLeft = GameObject.Find("LEFTbutton").GetComponent<Button>();
		Button btnRight = GameObject.Find("RIGHTbutton").GetComponent<Button>();

		randomYes=GameManager.buttonRandomization[GameManager.trial-1];

		if (randomYes == 1) 
		{
			btnLeft.GetComponentInChildren<Text>().text = "No";
			btnRight.GetComponentInChildren<Text>().text = "Yes";
			//btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(0));
		} 
		else 
		{
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
		}
	}



	//previouscities and addcity based on http://answers.unity3d.com/questions/906057/adding-gameobjects-to-a-list.html
	public List<int> previouscities = new List<int> ();

	void addcity(Item ItemToLocate)
	{
		if (previouscities.Count () == 0) {
			previouscities.Add (ItemToLocate.CityNumber);
			citiesvisited = previouscities.Count ();
		} else {
			previouscities.Add (ItemToLocate.CityNumber);
			citiesvisited = previouscities.Count ();
		}
	}

	public int citiesvisited = 0;  //does the .Count function need parentheses afterwrds? ()

	public static List<int> previouscities2f (List <int> previouscitiesP)
	{
		List<int> previouscities2 = previouscitiesP;
		previouscities2.RemoveAt (previouscitiesP.Count ()-1);
		return previouscities2;
	}

	public int distancetravelled()
	{
		int[] individualdistances = new int[previouscities.Count()];
		if (previouscities.Count() < 2) {
		} else {
			for (int i = 0; i < (previouscities.Count ()-1); i++) {
				individualdistances [i] = distances [previouscities[i], previouscities[i+1]];
			}
		}

		int distancetravelled = individualdistances.Sum ();
		distanceTravelledValue = distancetravelled;
		return distancetravelled;
	}
		
	void SetDistanceText ()
	{
		Debug.Log ("SetDistanceText");
		int distanceT = distancetravelled();
		DistanceText.text = "Distance so far: " + distanceT.ToString () + "km";
	}

	//if clicking on the first city, light it up. after that, clicking on a city will fill the destination city, indicating you've travelled to it, and draw a
	//connecting line between the city of departure and the destination
	void ClickOnCity(Item ItemToLocate)
	{
		if (!previouscities.Contains (ItemToLocate.CityNumber) || (previouscities.Count () == cities.Length && previouscities.First () == ItemToLocate.CityNumber)) {
			if (CityFirst (previouscities.Count ())) {
				LightFirstCity (ItemToLocate);
			} else {
				DrawLine (ItemToLocate);
			}
			addcity (ItemToLocate);
			itemClicks.Add (new Vector3 (ItemToLocate.CityNumber, GameManager.timeTrial - GameManager.tiempo,1));
			SetDistanceText ();
		} else if (previouscities.Last () == ItemToLocate.CityNumber) {
			EraseLine (ItemToLocate);
			itemClicks.Add (new Vector3 (ItemToLocate.CityNumber, GameManager.timeTrial - GameManager.tiempo,0));
		}
	}

	//determining whether the city is the first one to have been clicked in that instance i.e. where is the starting point
	bool CityFirst(int citiesvisited)
	{
		if (citiesvisited == 0)
		{
			return true;
		}
		else 
		{			
			return false;
		}
	}

	//turn the light on around the first city to be clicked on
	private void LightFirstCity(Item ItemToLocate)
	{
		
	
		Light myLight = ItemToLocate.gameItem.GetComponent<Light> ();
		myLight.enabled = !myLight.enabled;

		//int cityIn=(myLight.enabled)? 1 : 0 ;

	}

	//Reset.onClick.AddListener(ResetClicked);


	// The list of all the button clicks on items. Each event contains the following information:
	// ItemNumber (a number between 1 and the number of items. It corresponds to the index in the weight's (and value's) vector.)
	// Item is being selected In/Out (1/0) 
	// Time of the click with respect to the beginning of the trial 
	public static List <Vector3> itemClicks =  new List<Vector3> ();


	public GameObject[] lines= new GameObject[100];
	public LineRenderer[] newLine= new LineRenderer[100];

	void DrawLine(Item ItemToLocate) 
	{
		int cityofdestination = ItemToLocate.CityNumber;
		int cityofdeparture = previouscities[previouscities.Count()-1];

		Vector2 coordestination = unitycoord [cityofdestination];
		Vector2 coordeparture = unitycoord [cityofdeparture];

		Vector3[] coordinates = new Vector3[2];
		coordinates [0] = coordestination;
		coordinates [1] = coordeparture;
			
		GameObject instance = Instantiate (LineItemPrefab, new Vector2(0,0), Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		lines[citiesvisited] = instance;
		newLine[citiesvisited] = lines[citiesvisited].GetComponent<LineRenderer> ();
		newLine[citiesvisited].SetPositions(coordinates);
//		Debug.Log("LineCreated");
//		Debug.Log (citiesvisited);
		}

	//if double click on the previous city then cancel change the destination city back to vacant, and delete the connecting line b/w the two cities
	void EraseLine(Item ItemToLocate)
	{
//		Debug.Log("LineErased");
//		Debug.Log (citiesvisited);
		if (previouscities.Count == 1) {
			ItemToLocate.gameItem.GetComponent<Light> ().enabled = false;
		}
		Destroy (lines[citiesvisited-1]);
		previouscities.RemoveAt (previouscities.Count () - 1);
		citiesvisited --;
		SetDistanceText ();

//		Debug.Log ("Destroy");
	}
		

	private void Lightoff(){
		foreach(Item Item1 in Items){
			if (Item1.CityNumber == previouscities[0]){
				Light myLight = Item1.gameItem.GetComponent<Light> ();
				myLight.enabled = false;
			}
		}
	}

	public void ResetClicked(){
		for (int i = 0; i < lines.Length; i++) {
			DestroyObject (lines [i]);
		}
		Lightoff();
		previouscities.Clear();
		SetDistanceText ();
		citiesvisited = 0;
		itemClicks.Add (new Vector3 (100, GameManager.timeTrial - GameManager.tiempo,3));
		}
}
