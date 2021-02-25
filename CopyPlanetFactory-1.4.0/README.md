# CopyPlanetFactory

# 星球蓝图

按“复制C”复制当前星球的全部建筑

然后到另一个星球按“粘贴”能够复制建筑

暂时没有碰撞检测，只有重叠检测

补充物品：允许你将缺少物品补齐后，再次补全蓝图

保存的数据文件在 Dyson Sphere Program\BepInEx\config\PlanetFactoryData


Press "copy" to copy all the buildings on the current planet

Then go to another planet and press "paste" to copy the building

The test version has no collision detection for the time being, only overlap detection

replenish Item: Allow you to replenish the blueprint after completing the missing items

data file in Dyson Sphere Program\BepInEx\config\PlanetFactoryData


### 镜像粘贴
赤道镜像粘贴：将蓝图中的建筑以赤道为镜像建造，赤道为Y轴
东西镜像粘贴：将蓝图中的建筑以东西半球为镜像建造，东西半球为为Z轴
左右镜像粘贴：将蓝图中的建筑以左右为镜像建造，左右半球为X轴
XY：先x轴镜像再y轴镜像，后面同理

### Mirror paste

Equator mirror paste: the building in the blueprint is built with the equator as the mirror image, and the equator is the Y axis

EW Mirror past: the building in the blueprint is built with the east and west hemisphere as the mirror image, and the east and west hemisphere is the Z axis

LF Mirror paste: the building in the blueprint is built with the left and right as mirror images, and the left and right hemispheres are the X axis

XY: Mirror on the x-axis and then on the y-axis, and the same applies later

### 区域选择

你可以选择区域进行粘贴
目前区域只有8块
按照赤道，南北，左右划分

### area select

You can select the area to paste
Currently there are only 8 blocks in the area
Divided by the equator, north and south, left and right

### Installation

1. Install BepInEx
3. Then drag Tp.dll into steamapps/common/Dyson Sphere Program/BepInEx/plugins


### 安装

1. 先安装 BepInEx框架
3. 将CopyPlanetFactory.dll拖到 steamapps/common/Dyson Sphere Program/BepInEx/plugins文件夹内
