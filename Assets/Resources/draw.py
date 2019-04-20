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

def reset(xlabel = 'No. of Nodes',maxy = 100,ylabel='No. of Nodes',title='Nodes expanded in CT'):
    plt.title(title)
    plt.xlabel(xlabel)
    plt.ylabel(ylabel)

    # plt.xlim(0,100)
    # plt.ylim(0,maxy)

def save(path):
    plt.savefig(path,dpi=300)
    plt.close()

size = 0.1

    
vcost = getStats('VH.txt')
normal = getStats('N.txt')

# stat1 = normal 
# stat2 = vcost

y1 = normal[0]
y2 = vcost[0][:len(y1)]
y1 = y1[:len(y2)]

lowerlimit = 5
upperlimit = 200

y2 = [y2[i] for i in range(len(y2)) if y1[i]>lowerlimit and y1[i]<upperlimit]
y1 = [y1[i] for i in range(len(y1)) if y1[i]>lowerlimit and y1[i]<upperlimit]

X = list(range(len(y1)))


CA_CBS = 'CA-CBS'
CBS = 'Normal CBS'

CA_CBS_COLOR = 'black'
CBS_COLOR = 'blue'

# simple sequential compare
width = 1

plt.title(CA_CBS+' vs '+CBS)
plt.xlabel('Problem ID')
plt.ylabel('No. of Nodes')

n = 300
plt.bar(X[:n], y1[:n],label=CBS,width=0.9,linewidth=0)
plt.bar(X[:n], y2[:n],label=CA_CBS, color=CA_CBS_COLOR,width=0.5,linewidth=0)
# plt.show()
plt.legend()
save('results/seq_nodes.png')


# sequential ratio compare
width = 1

plt.title('Node Ratio ('+CA_CBS+' to '+CBS+')')
plt.xlabel('Problem ID')
plt.ylabel('Ratio of no. of nodes in CT')
n = 500
plt.ylim(0,1.5)
# plt.plot(X[:n], [1 for i in X[:n]],label=CBS,linewidth=0.4)
plt.bar(X[:n], np.array(y2)[:n]/np.array(y1)[:n],label=CA_CBS, color=CA_CBS_COLOR,width=0.5,linewidth=0)
# plt.show()
save('results/seq_ratio.png')



# ratio distribution bar graphs

y1 = np.array(y1)
y2 = np.array(y2)

reset(maxy=1.2, xlabel = 'Ratio',ylabel='Count',title='Ratio distribution ('+CA_CBS+' vs '+CBS+')')


unique, counts = np.unique(np.around(y2/y1,decimals=1), return_counts = True)
unique,counts = unique[:-2],counts[:-2]

plt.bar(np.arange(len(counts)), counts, linewidth=1, edgecolor = 'white')
plt.xticks(np.arange(len(counts)), unique)
save('results/distribution_ratio.png')






########## SORTING BASED ON Y1

y1, y2 = zip(*sorted(zip(y1, y2)))

y1 = np.array(y1)
y2 = np.array(y2)



# scatter plot CA-CBS vs Normal CBS

reset()
plt.plot(y1, y1,label=CBS,linewidth=width)
plt.scatter(y1, y2, label=CA_CBS,color=CA_CBS_COLOR, s=size)
plt.legend()
save('results/scatter_nodes.png')




################ AVERAGE MERGER

y1, y2 = [np.array(arr) for arr in merger(y1, y2)]





# line plot CA-CBS vs Normal CBS


reset()
plt.plot(y1, y1,label=CBS,linewidth=width)
plt.plot(y1, y2, color=CA_CBS_COLOR,label=CA_CBS,linewidth=width)
plt.legend()
save('results/avg_nodes.png')


# ratio vs node count plot 

reset(maxy=1.2, ylabel='Ratio of No. of nodes expanded',title='Average Node Ratio ('+CA_CBS+' to '+CBS+')')

plt.plot(y1, [1 for i in y1],label=CBS,linewidth=0.4)
plt.bar(y1, y2/y1,label=CA_CBS, color=CA_CBS_COLOR,linewidth=1, edgecolor = 'white')

plt.legend()
save('results/avg_ratio.png')


