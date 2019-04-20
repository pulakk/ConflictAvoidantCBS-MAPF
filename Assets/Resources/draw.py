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
    
vcost = getStats('VH.txt')
normal = getStats('N.txt')

stat1 = normal 
stat2 = vcost

x1 = list(range(len(stat1[0])))
x2 = list(range(len(stat2[0,:len(x1)])))

# x1 = stat1[1]
# x2 = stat2[1]

y1 = stat1[0]
y2 = stat2[0][:len(y1)]

y1, y2 = zip(*sorted(zip(y1, y2)))

size = 0.8


def reset(xlabel = 'No. of Nodes',maxy = 100,ylabel='No. of Nodes',title='Nodes expanded in CT'):
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)

    plt.xlim(0,100)
    plt.ylim(0,maxy)

def save(path):
    plt.legend()
    plt.savefig(path)
    plt.close()

# scatter plot

reset()
plt.plot(y1, y1,label='Normal',linewidth=size)
plt.scatter(y1, y2, label='V-Heuristic',color='red', s=size)
save('results/scatter_plot.png')



# line plot

y1, y2 = [np.array(arr) for arr in merger(y1, y2)]

reset()
plt.plot(y1, y1,label='Normal',linewidth=size)
plt.plot(y1, y2, color='red',label='V-Heuristic',linewidth=size)
save('results/line_plot_avg.png')





# # # line plot

reset(maxy=1, ylabel='Ratio of No. of nodes expanded',title='Improvement Ratio vs No. of nodes in CT')

# plt.plot(y1, y1/,label='Normal',linewidth=size)
plt.scatter(y1, y2/y1,label='V-Heuristic',s=size)

save('results/ratio.png')