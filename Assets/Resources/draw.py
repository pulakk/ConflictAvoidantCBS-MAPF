import matplotlib.pyplot as plt 
import numpy as np

def getStats(path):
    with open(path,'r') as f:
        stats = f.readlines()

        stats = [row.strip().split(',') for row in stats]
        stats = np.array([[int(val) for val in row] for row in stats])
        stats = np.transpose(stats)
    return stats

def merger(x, y):
    return zip(*sorted((xVal, np.mean([yVal for a, yVal in zip(x, y) if xVal==a])) for xVal in set(x)))
    
vcost = getStats('log.txt')
normal = getStats('log - Copy.txt')

stat1 = normal 
stat2 = vcost

x1 = list(range(len(stat1[0])))[:34]
x2 = list(range(len(stat2[0])))[:34]

# x1 = stat1[1]
# x2 = stat2[1]

y1 = stat1[0][:34]
y2 = stat2[0][:34]

size = 3

plt.xlim(0,34)
# plt.scatter(y1, y1, label='Normal', s=size)
plt.scatter(y1, y2, label='V-Heuristic',color='red', s=size)

# plt.plot(x1, y1,label='Normal',linewidth=size)
# plt.plot(x2, y2, color='red',label='V-Heuristic',linewidth=size)
plt.legend()
plt.show()
# plt.savefig('results/performance.png')
plt.close()