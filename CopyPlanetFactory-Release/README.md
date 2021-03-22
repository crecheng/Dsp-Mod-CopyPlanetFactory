# CopyPlanetFactory

# 星球蓝图

按“复制C”复制当前星球的全部建筑

然后到另一个星球按“粘贴”能够复制建筑

暂时没有碰撞检测，只有重叠检测

会自动从当前星球的箱子获取建造物品

保存的数据文件在 Dyson Sphere Program\BepInEx\config\PlanetFactoryData


Press "copy" to copy all the buildings on the current planet

Then go to another planet and press "paste" to copy the building

The test version has no collision detection for the time being, only overlap detection

Will automatically obtain construction items from the chests of the current planet

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

### 更新
1.7.1 更新数据格式，加入复制分馏塔

2.0.0 全新ui，修复bug，加入区域框选复制

2.1.0 修复bug，加入区域删除，加入箱子复制

2.1.1 修复bug

2.1.3 修复bug

2.1.5 更新传送带建造逻辑

2.2.0 更新连接逻辑，取消补充物品功能，但是会从当前星球的箱子里获取物品

2.2.1 物品不足建无法粘贴，加入撤销功能

2.2.4 修复bug

2.2.5 修复bug，蓝图加入描述

2.3.0 修复bug，支持高层研究塔

2.3.1 修复重复计算研究所的bug

2.3.2 修复了高层研究所会出现bug

2.3.4 加入移除无连接爪子，加入尝试连接断开的传送带