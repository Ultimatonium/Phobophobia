using UnityEngine;

public class PlayerAnimFootsteps : MonoBehaviour
{
  private enum CURRENT_TERRAIN {GRASS, GRAVEL, WOOD_FLOOR, WATER};

  [SerializeField] private CURRENT_TERRAIN currentTerrain;

  private FMOD.Studio.EventInstance foosteps;

  private void DetermineTerrain()
  {
    RaycastHit[] hit;
    hit = Physics.RaycastAll(transform.position, Vector3.down, 10f);

    foreach(RaycastHit rayHit in hit)
    {
      if(rayHit.transform.gameObject.layer == LayerMask.NameToLayer("Gravel"))
      {
        currentTerrain = CURRENT_TERRAIN.GRAVEL;
        break;
      }
      else if(rayHit.transform.gameObject.layer == LayerMask.NameToLayer("Wood"))
      {
        currentTerrain = CURRENT_TERRAIN.WOOD_FLOOR;
        break;
      }
      else if(rayHit.transform.gameObject.layer == LayerMask.NameToLayer("Grass"))
      {
        currentTerrain = CURRENT_TERRAIN.GRASS;
        break;
      }
      else if(rayHit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
      {
        currentTerrain = CURRENT_TERRAIN.WATER;
        break;
      }
    }
  }

  private void Update()
  {
    DetermineTerrain();
  }

  private void PlayFootstep(int terrain)
  {
    foosteps = FMODUnity.RuntimeManager.CreateInstance("event:/Footsteps");
    foosteps.setParameterByName("Terrain", terrain);
    foosteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
    foosteps.start();
    foosteps.release();
  }

  public void SelectAndPlayFootstep()
  {
    switch(currentTerrain)
    {
      case CURRENT_TERRAIN.GRASS:
        PlayFootstep(0);
        break;
      case CURRENT_TERRAIN.GRAVEL:
        PlayFootstep(1);
        break;
      case CURRENT_TERRAIN.WOOD_FLOOR:
        PlayFootstep(2);
        break;
      case CURRENT_TERRAIN.WATER:
        PlayFootstep(3);
        break;
    }
  }
}
