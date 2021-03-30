using UnityEngine;

public class ChickensController : AnimalsController
{
    public static ChickensController Instance;
    public int starved = 0, eaten = 0;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        animals = new Chicken[numAnimals];
        totalAnimals = numAnimals;
        hexGrid = HexGrid.Instance;
        geneticAlgorithm = GetComponent<GeneticAlgorithm>();
        SpawnChickens();
    }

    private void SpawnChickens()
    {
        for (int i = 0; i < numAnimals; i++)
        {
            GameObject chickenObj = Instantiate(animalPrefab);
            Chicken chicken = chickenObj.AddComponent<Chicken>();
            chickenObj.name = "Chicken" + i;
            animals[i] = chicken;
            chickenObj.transform.position = GetRandomSpawnLocation(chicken);
            chickenObj.transform.LookAt(new Vector3(0, chickenObj.transform.position.y, 0));
            chicken.SetBaseStats(speed, strength, vision, energy, puerperal, Animal.SpeciesType.Prey, false);    // starter statistics ( TO BE CHANGED BY USER)
            chicken.transform.parent = this.transform;
        }
    }

    public Vector2Int IsChickenNear(Vector2Int[] tiles, Chicken currentChicken)
    {
        foreach (Vector2Int tile in tiles)
        {
            foreach (var chicken in animals)
            {
                if (chicken != currentChicken)
                {
                    if (chicken.location == tile)
                    {
                        return tile;
                    }
                }
            }
        }
        return new Vector2Int(-1, -1);
    } 

}
