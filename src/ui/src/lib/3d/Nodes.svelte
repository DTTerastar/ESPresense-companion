<script lang="ts">
    import { onDestroy } from 'svelte';
    import { nodes } from '$lib/stores';
    import * as THREE from 'three';
    import { CSS2DObject } from 'three/examples/jsm/renderers/CSS2DRenderer.js';
    import type { Group } from 'three';
    import type { Node } from '$lib/types';

    export let groupPivot: Group;
    export let enabled = true;

    // Position adjustments
    const X_POS_ADJ = 1.5;
    const Y_POS_ADJ = 5;

    const nodeMaterials = {
        online: new THREE.MeshPhongMaterial({
            color: 0x000000,
            emissive: 0x5555ff,
            emissiveIntensity: 2,
            shininess: 100,
            toneMapped: false
        }),
        offline: new THREE.MeshPhongMaterial({
            color: 0x000000,
            emissive: 0xff2222,
            emissiveIntensity: 2,
            shininess: 100,
            toneMapped: false
        }),
    };

    let nodeGroup: THREE.Group | null = null;

    $: if ($nodes && groupPivot && enabled) {
        updateNodes($nodes);
    }

    $: if (!enabled) {
        cleanupNodeGroup();
    }

    function cleanupNodeGroup() {
        if (!nodeGroup) return;

        nodeGroup.traverse(child => {
            if ((child as any).geometry) {
                (child as any).geometry.dispose();
            }
            if ((child as any).material) {
                (child as any).material.dispose();
            }
            // Clean up CSS2D labels
            if (child instanceof CSS2DObject) {
                const element = child.element;
                if (element && element.parentNode) {
                    element.parentNode.removeChild(element);
                }
            }
        });

        groupPivot.remove(nodeGroup);
        nodeGroup = null;
    }

    function updateNodes(nodes: Node[]) {
        cleanupNodeGroup();

        const newNodeGroup = new THREE.Group();
        newNodeGroup.name = 'NodeGroup';

        nodes.forEach((node) => {
            if (!node.location) {
                console.warn('Node missing location:', node);
                return;
            }

            // Create mesh with emissive material for glow
            const mesh = new THREE.Mesh(
                new THREE.SphereGeometry(0.08, 32, 16),
                nodeMaterials[node.online ? 'online' : 'offline']
            );

            mesh.position.set(
                node.location.x - X_POS_ADJ,
                node.location.y - Y_POS_ADJ,
                node.location.z
            );
            mesh.name = "node#" + node.id;

            // Add mesh and label to group
            newNodeGroup.add(mesh);
            newNodeGroup.add(createLabelForNode(node));
        });

        nodeGroup = newNodeGroup;
        groupPivot.add(nodeGroup);
    }

    function createLabelForNode(node: Node) {
        const labelDivEle = document.createElement('div');
        labelDivEle.style.color = node.online ? '#5555ff' : '#dc2d2d';
        labelDivEle.style.fontFamily = 'Arial';
        labelDivEle.style.fontSize = '0.8rem';
        labelDivEle.style.marginTop = '-1em';

        const labelDivLine1 = document.createElement('div');
        labelDivLine1.textContent = node.name;
        labelDivEle.append(labelDivLine1);

        const labelElement = new CSS2DObject(labelDivEle);
        labelElement.name = "nodeLabel";

        return labelElement;
    }

    onDestroy(() => {
        cleanupNodeGroup();
    });
</script>