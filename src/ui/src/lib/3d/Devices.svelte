<script lang="ts">
    import { T, useFrame } from '@threlte/core';
    import { HTML } from '@threlte/extras';
    import { devices, config } from '$lib/stores';
    import type { Device } from '$lib/types';
    import * as THREE from 'three';

    // Position adjustments
    const X_POS_ADJ = 1.5;
    const Y_POS_ADJ = 5;
    const FLOOR_HEIGHT = 3;
    const DEVICE_HEIGHT = 0.5;

    // Animation settings
    const PULSE_SPEED = 2;  // Oscillations per second
    const PULSE_MIN = 0.8;
    const PULSE_MAX = 1.2;

    const trackerMaterials = [
        {
            emissive: 0xff0000,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        },
        {
            emissive: 0xffbb00,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        },
        {
            emissive: 0xffee00,
            emissiveIntensity: 2,
            transparent: true,
            opacity: 0.8
        }
    ];

    let scale = 1;
    let startTime = Date.now();

    $: floors = $config?.floors || [];
    $: floorMap = new Map(floors.map((f, i) => [f.id, i]));

    function getDeviceFloorIndex(device: Device): number {
        if (!device.floor?.id) return 0;
        return floorMap.get(device.floor.id) || 0;
    }

    useFrame(() => {
        const elapsed = (Date.now() - startTime) / 1000;
        const phase = (elapsed * PULSE_SPEED * Math.PI) % (Math.PI * 2);
        scale = PULSE_MIN + (Math.sin(phase) + 1) * (PULSE_MAX - PULSE_MIN) / 2;
    });
</script>

<T.Group name="DeviceGroup">
    {#each $devices as device, i}
        {#if device.location && device.confidence > 1}
            {@const floorIndex = getDeviceFloorIndex(device)}
            <T.Group
                position={[
                    device.location.x - X_POS_ADJ,
                    device.location.y - Y_POS_ADJ,
                    floorIndex * FLOOR_HEIGHT + DEVICE_HEIGHT
                ]}
                scale={[scale, scale, scale]}
            >
                <!-- Device sphere -->
                <T.Mesh name={device.id}>
                    <T.SphereGeometry args={[0.2, 32, 16]} />
                    <T.MeshStandardMaterial {...trackerMaterials[i % trackerMaterials.length]} />
                </T.Mesh>

                <!-- Connection line to floor -->
                <T.Mesh position.z={-DEVICE_HEIGHT / 2}>
                    <T.CylinderGeometry args={[0.02, 0.02, DEVICE_HEIGHT, 8]} />
                    <T.MeshBasicMaterial
                        color={trackerMaterials[i % trackerMaterials.length].emissive}
                        transparent={true}
                        opacity={0.3}
                    />
                </T.Mesh>

                <!-- Device label -->
                <HTML
                    center
                    occlude
                    position.y={0.4}
                    style="color: #ffffff;
                           font-family: Arial;
                           font-size: 0.8rem;
                           font-weight: bold;
                           text-shadow: 0 0 4px rgba(0,0,0,0.5);"
                >
                    <div>
                        {device.name?.length > 15 ? device.name.substring(0, 14) + '...' : device.name || device.id}
                    </div>
                    <div>
                        {device.confidence}% ({device.fixes} fixes)
                    </div>
                </HTML>
            </T.Group>
        {/if}
    {/each}
</T.Group>