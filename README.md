# MLAgents_Projects
This repository is for my personal ML-Agents projects, and also acts as a testbed for work I am doing with OxAI (see 'vrai' repo).

## Navigation Agents

In this project I created an environment in Unity to teach an agent to navigate a simple maze via imitation learning (Behavioural Cloning and Generative Adversarial Imitation Learning), and also tested how well it generalises by placing it in a more complicated environment that it had never seen before.

The agent has access to a 84x84 camera feed, and 10 raycasts spread evenly around its body, which read in the distance to the obsered object.

The agent was trained in this simple environment, which is essentially a one-dimensional closed loop. The training algorithm generated a poilcy to navigate this environment in about 100k timesteps. An example of its behaviour can be seen here

![](https://media.giphy.com/media/UqAPowf0hwU4Z0chQN/giphy.gif)

To test how well this policy generalises, I created a more complex enviornment, full of dead-ends, narrow passages, rooms, etc. to my delight, the policy seems to generalise quite well! An example of its behaviour can be seen here

![](https://media.giphy.com/media/fXu93GBEhoeneGAfkr/giphy.gif)

Interestingly, the agent can learn without the use of raycasts, however it's performance suffers, particularly near corners and dead-ends, unlike for the agent with raycasts. An example of its performance in the more complex environment can be seen here

![](https://media.giphy.com/media/W1OFu87rwkb1ADtn6J/giphy.gif)


This is surprising, given the similar training statistics

![](https://i.imgur.com/d32BXrE.png)
