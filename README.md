# SAVE-Kauri-tree
Simulation and Visualisation of Effect of Climate Change on Kauri Trees (SAVE Kauri) 

# Chosen Procedural Modeling Method
The Space Colonization Algorithm was chosen to model the trees.

A few changes to the algorithm are necessary to better suit Kauri and our purposes. Due to the non-tapering nature of mature Kauri trunks, a new function for determining branch and trunk diameter in the model is needed. Additionally, due to the importance of roots in the transmission of dieback, we wanted our model to incorporate the root systems of the trees.

# Running our visualization
The Unity project uses Unity 2019.4.29f1

There are 3 different scenes:
- Procedural: Contains just the SCA, press run to generate trees
- Hand Made: All the hand made models
- Comparison: Contains both hand made and procedural. Press run to generate the procedural.

The procedural models are currently just skeletons that get leaves, there is no mesh for the trunk. The skeletons are visible in the scene view if gizmos are enabled.
It is reccomened to use the scene view to inspect the models as there is no camera control in the game view.

The branch `mesh-problem` has an implementation for generating meshes, but it is not finalized.
