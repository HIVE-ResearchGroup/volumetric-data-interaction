using Assets.Scripts.Exploration;
using UnityEngine;

public class ModelTest : MonoBehaviour
{
    private Model model;
    public GameObject modelGo;

    void Start()
    {
        Debug.Log(modelGo);
        model = new Model();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetIntersectionImage(10, 10, 10, 10);
        }
    }

    private void SetIntersectionImage(int zTopLeft, int zTopRight, int zBottomRight, int zBottomLeft)
    {
        //var intersection = model.CalculateCuttingplane(zTopLeft, zTopRight, zBottomRight, zBottomLeft);
        //var fileName = $"{zTopLeft}x{zTopRight}x{zBottomRight}x{zBottomLeft}";
        
        //var fileLocation = Path.Combine(ConfigurationConstants.IMAGES_FOLDER_PATH, fileName + "jpeg");
        //intersection.Save(fileLocation, ImageFormat.Jpeg);

        //var resourceImage = Path.Combine(StringConstants.Images, fileName);
        //Texture2D testImage = Resources.Load(resourceImage) as Texture2D;
        //gameObject.GetComponent<MeshRenderer>().material.mainTexture = testImage;
    }
}
