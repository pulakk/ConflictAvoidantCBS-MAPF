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
    
stat1 = getStats('log.txt')
stat2 = getStats('log - Copy.txt')

x1 = list(range(len(stat1[0])))
x2 = list(range(len(stat2[0])))

# x1 = stat1[1]
# x2 = stat2[1]

y1 = stat1[0]
y2 = stat2[0]

# plt.ylim(0,200)
# plt.scatter(x1, y1, s=0.8)
# plt.scatter(x2, y2, color='red', s=0.8)

plt.plot(x1, y1,label='Normal',linewidth=0.5)
plt.plot(x2, y2, color='red',label='V-Heuristic',linewidth=0.5)
plt.legend()
plt.show()
plt.close()