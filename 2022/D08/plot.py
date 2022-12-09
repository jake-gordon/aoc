#!/usr/bin/env python
import numpy as np
import matplotlib.pyplot as plt
plt.rcParams.update({"text.usetex": True})
from mpl_toolkits.axes_grid1 import make_axes_locatable

with open('./map.visible') as file: visibleMap = np.array([[int(x) for x in row.split()] for row in file])
with open('./map.scenic') as file: scenicMap = np.array([[int(x) for x in row.split()] for row in file])

fig, ax = plt.subplots(1,3,figsize=(12,36))
im0 = ax[0].imshow(visibleMap, interpolation="nearest", origin="upper",
                   cmap=plt.cm.gray.reversed())
divider0 = make_axes_locatable(ax[0])
cax0 = divider0.append_axes('right', size='5%', pad=0.05)
cbar0 = plt.colorbar(im0,cax=cax0,orientation="vertical")
cbar0.set_ticks([0,1]);
cbar0.set_ticklabels(["no","yes"]);
ax[0].set_xticks([]);
ax[0].set_yticks([]);
ax[0].set_xticklabels([]);
ax[0].set_yticklabels([]);
ax[0].set_title("Visible from Outside");
im1 = ax[1].imshow(scenicMap, interpolation="nearest", origin="upper",
                   cmap=plt.cm.plasma,vmin=np.amin(scenicMap),vmax=np.amax(scenicMap))
divider1 = make_axes_locatable(ax[1])
cax1 = divider1.append_axes('right', size='5%', pad=0.05)
plt.colorbar(im1,cax=cax1,orientation="vertical")
ax[1].set_xticks([]);
ax[1].set_yticks([]);
ax[1].set_xticklabels([]);
ax[1].set_yticklabels([]);
ax[1].set_title("Scenic Score");
weightedScenicMap = scenicMap*(1-visibleMap)
im2 = ax[2].imshow(weightedScenicMap, interpolation="nearest", origin="upper",
                   cmap=plt.cm.plasma,vmin=np.amin(weightedScenicMap),vmax=np.amax(weightedScenicMap))
divider2 = make_axes_locatable(ax[2])
cax2 = divider2.append_axes('right', size='5%', pad=0.05)
plt.colorbar(im2,cax=cax2,orientation="vertical")
ax[2].set_xticks([]);
ax[2].set_yticks([]);
ax[2].set_xticklabels([]);
ax[2].set_yticklabels([]);
ax[2].set_title("Scores for Invisible Sites");
plt.tight_layout()
plt.savefig("./map.png",bbox_inches="tight",dpi=600)
# plt.show()
